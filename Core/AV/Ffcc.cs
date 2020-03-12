using System.ComponentModel;
using System.Diagnostics;

namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Graphics;
    using NAudio.Wave;
    using NAudio.Wave.SampleProviders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Ffcc is a front end for processing Audio and Video using ffmpeg.autogen
    /// </summary>
    /// <remarks>
    /// Code bits mostly converted to c# from c++ It uses bits of code from FFMPEG examples and a-forge,
    /// FFMPEG.autogen, stack overflow
    /// </remarks>
    public abstract class Ffcc : IDisposable
    {

        protected static unsafe T Load<T>(BufferData* bufferData, byte[] headerData, int loopStart, FfccMode ffccMode, AVMediaType avType) where T : Ffcc, new()
        {
            if (bufferData == null) throw new ArgumentNullException(nameof(bufferData));
            if (!Enum.IsDefined(typeof(FfccMode), ffccMode))
                throw new InvalidEnumArgumentException(nameof(ffccMode), (int) ffccMode, typeof(FfccMode));
            if (!Enum.IsDefined(typeof(AVMediaType), avType))
                throw new InvalidEnumArgumentException(nameof(avType), (int) avType, typeof(AVMediaType));
            T r = new T();

            void play(BufferData* d)
            {
                r.LoadFromRam(d);
                r.Init(null, avType, ffccMode, loopStart);
                if (ffccMode != FfccMode.ProcessAll) return;
                ffmpeg.avformat_free_context(r.Decoder.Format);
                //ffmpeg.avio_context_free(&Decoder._format->pb); //CTD
                r.Decoder.Format = null;
                r.Dispose(false);
            }
            if (headerData != null)
                fixed (byte* tmp = &headerData[0])
                {
                    lock (r.Decoder)
                    {
                        bufferData->SetHeader(tmp);
                        play(bufferData);
                    }
                }
            else
                play(bufferData);
            return r;
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        protected static T Load<T>(string filename, AVMediaType mediaType = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.StateMach, int loopStart = -1) where T : Ffcc, new()
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            if (!Enum.IsDefined(typeof(AVMediaType), mediaType))
                throw new InvalidEnumArgumentException(nameof(mediaType), (int) mediaType, typeof(AVMediaType));
            if (!Enum.IsDefined(typeof(FfccMode), mode))
                throw new InvalidEnumArgumentException(nameof(mode), (int) mode, typeof(FfccMode));
            T r = new T();
            r.Init(filename, mediaType, mode, loopStart);
            if (mode == FfccMode.ProcessAll)
                r.Dispose(false);
            return r;
        }

        #region Fields

        /// <summary>
        /// If you have sound skipping increase this number and it'll go away. might decrease sync or
        /// increase memory load The goal is to keep the dynamic sound effect interface fed. If it plays
        /// the audio before you give it more, then you get sound skips.
        /// </summary>
        /// <value>The goal buffer Count.</value>
        /// <remarks>
        /// Will want to be as low as possible without sound skipping. 91.875 is 1 second of audio at
        /// 44100 hz @ 15 fps; 99.9001 is 1 second of audio at 48000 hz @ 15 fps;
        /// </remarks>
        private const int GoalBufferCount = 100;

        /// <summary>
        /// NextAsync sleeps when filling buffer. If set too high buffer will empty before filling it again.
        /// </summary>
        private const int NextAsyncSleep = 10;

        /// <summary>
        /// on exception this is turned to true and will force fallback to naudio when monogame isn't working.
        /// </summary>
        private static bool _useNAudio;

        private readonly object _dynamicSoundEffectLock = new object();
        private readonly unsafe AVDictionary* _dict;
        private unsafe AVIOContext* _avioCtx;
        private unsafe byte* _avioCtxBuffer;
        private int _avioCtxBufferSize;
        private byte[] _convertedData;

        //private byte* _convertedData;
        private MemoryStream _decodedMemoryStream;

        private bool _frameSkip = true;

        //private IntPtr _intPtr;
        private int _loopStart;


        private CancellationToken _cancellationToken = new CancellationToken();
#if _WINDOWS
        private BufferedWaveProvider _bufferedWaveProvider;

        /// <summary>
        /// Wave out for naudio only works in windows.
        /// </summary>
        /// <see cref="https://markheath.net/post/how-to-record-and-play-audio-at-same"/>
        private DirectSoundOut _nAudioOut;

        /// <summary>
        /// DirectSound requires VolumeSampleProvider to change volume.
        /// </summary>
        private VolumeSampleProvider _volumeSampleProvider;

        /// <summary>
        /// Required by naudio to pan the sound.
        /// </summary>
        private PanningSampleProvider _panningSampleProvider;

#endif
        private avio_alloc_context_read_packet _rf;
        private avio_alloc_context_seek _sf;
        private CancellationTokenSource _sourceToken;


        private Task _task;
        private DynamicSoundEffectInstance _dynamicSound;

        #endregion Fields

        #region Destructors

        ~Ffcc()
        {
            Dispose(false);
        }

        #endregion Destructors

        #region Enums

        public enum FfccMode
        {
            /// <summary>
            /// Processes entire file at once and does something with output
            /// </summary>
            ProcessAll,

            /// <summary>
            /// State machine, functions in this call update to update current state. And update
            /// decides what to do next.
            /// </summary>
            StateMach,

            /// <summary>
            /// Not Init some error happened that prevented the class from working
            /// </summary>
            NotInit
        }

        public enum FfccState
        {
            Open,

            /// <summary>
            /// Waiting for request for next frame
            /// </summary>
            Waiting,

            /// <summary>
            /// Read all the data
            /// </summary>
            ReadAll,

            /// <summary>
            /// Done reading file nothing more can be done
            /// </summary>
            Done,

            /// <summary>
            /// Don't change state just pass ret value.
            /// </summary>
            Null,

            /// <summary>
            /// Get packet of data containing frames
            /// </summary>
            ReadOne,

            /// <summary>
            /// Missing DLL required to function
            /// </summary>
            NoDLL,

            /// <summary>
            /// Gets stream and Codec
            /// </summary>
            GetCodec,

            /// <summary>
            /// Prepares Scaler for Video stream
            /// </summary>
            PrepareScaler,

            /// <summary>
            /// Start Reading
            /// </summary>
            Read,

            /// <summary>
            /// Prepares Resampler for Audio stream
            /// </summary>
            PrepareResampler
        }

        #endregion Enums

        #region Properties


        /// <summary>
        /// Are you ahead of target frame
        /// </summary>
        /// <returns>true if ahead</returns>
        public bool Ahead
        {
            get
            {
                if (Decoder.StreamIndex == -1 || Mode != FfccMode.StateMach) return true;
                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    if (DynamicSound != null)
                    {
                        return DynamicSound.PendingBufferCount > GoalBufferCount;
                    }
#if _WINDOWS

                    if (_useNAudio)
                    {
                        return _bufferedWaveProvider.BufferedDuration.TotalSeconds > _bufferedWaveProvider.BufferDuration.TotalSeconds - 1;
                    }
#endif
                }
                else if (Timer.IsRunning)
                {
                    return CurrentFrameNum > ExpectedFrame;
                }
                return true;
            }
        }

        /// <summary>
        /// Are you behind target frame
        /// </summary>
        /// <returns>true if behind</returns>
        public bool Behind => !Ahead && !Current;

        /// <summary>
        /// Are you on target frame
        /// </summary>
        /// <returns>true if correct frame</returns>
        public unsafe bool Current
        {
            get
            {
                if (Decoder.StreamIndex != -1 && Mode == FfccMode.StateMach)
                {
                    if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                    {
                        if (DynamicSound != null)
                        {
                            return DynamicSound.PendingBufferCount == GoalBufferCount;
                        }
#if _WINDOWS

                        if (_useNAudio)
                        {
                            return Math.Abs(_bufferedWaveProvider.BufferedDuration.TotalSeconds - (_bufferedWaveProvider.BufferDuration.TotalSeconds - 1)) < float.Epsilon;
                        }
#endif

                        Die($"{Decoder.CodecContext->sample_rate} is currently unsupported");
                    }
                    else if (Timer.IsRunning)
                    {
                        return CurrentFrameNum == ExpectedFrame;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Path and filename of file.
        /// </summary>
        public string DecodedFileName { get; set; }

        /// <summary>
        /// Dynamic Sound Effect Interface for class allows control out of class. Mode must be in STATE_MACH
        /// </summary>
        public DynamicSoundEffectInstance DynamicSound
        {
            get => _dynamicSound; private set
            {
                lock (_dynamicSoundEffectLock)
                    _dynamicSound = value;
            }
        }

        /// <summary>
        /// True if file is open.
        /// </summary>
        public bool FileOpened { get; set; }

        /// <summary>
        /// returns Frames per second or if that is 0. it will return the Time_Base ratio. This is
        /// the fundamental unit of time (in seconds) in terms of which frame timestamps are
        /// represented. In many cases the audio files time base is the same as the sample rate.
        /// example 1/44100. video files audio stream might be 1/100 or 1/1000. these can make for
        /// large durations.
        /// </summary>
        public unsafe double FPS
        {
            get
            {
                double r = FPSVideo;
                if (Math.Abs(r) > double.Epsilon)
                {
                    return r;
                }

                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO && Decoder.CodecContext != null && Decoder.CodecContext->framerate.den != 0)
                {
                    return Decoder.CodecContext->framerate.num / (double)Decoder.CodecContext->framerate.den;
                }

                if (Decoder.Stream != null && Decoder.Stream->time_base.den != 0)
                {
                    return Decoder.Stream->time_base.num / (double)Decoder.Stream->time_base.den; // returns the time_base
                }

                return 0;
            }
        }

        /// <summary>
        /// When getting video frames if behind it goes to next frame. disabled for debugging purposes.
        /// </summary>
        public bool FrameSkip { get => MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO && _frameSkip; set => _frameSkip = value; }

        /// <summary>
        /// Is the class disposed of. If true calling Dispose() does nothing.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Sample Count that loop starts from.
        /// </summary>
        public int LoopStart { get => _loopStart; set => _loopStart = value; }

        /// <summary>
        /// Current media type being processed.
        /// </summary>
        public AVMediaType MediaType { get; set; }

        /// <summary>
        /// Metadata container for tags.
        /// </summary>
        public Dictionary<String, String> Metadata { get;  } = new Dictionary<string, string>();

        /// <summary>
        /// SoundEffect for class allows control out of class. Mode must be in PROCESS_ALL
        /// </summary>
        public SoundEffect SoundEffect { get; set; }

        /// <summary>
        /// SoundEffectInterface for class. allows for more control than just playing the above sound effect.
        /// </summary>
        public SoundEffectInstance SoundEffectInstance { get; set; }

        /// <summary>
        /// Stopwatch tracks the time audio has played so video can sync or loops can be looped.
        /// </summary>
        public Stopwatch Timer { get; } = new Stopwatch();

        /// <summary>
        /// if there is no stream it returns false. only checked when trying to process audio
        /// </summary>
        private bool AudioEnabled => Decoder.StreamIndex >= 0;

        //private byte* ConvertedData { get => _convertedData; set => _convertedData = value; }
        private byte[] ConvertedData { get => _convertedData; set => _convertedData = value; }

        /// <summary>
        /// Current frame number
        /// </summary>
        /// <returns>Current frame number or -1 on error</returns>
        public unsafe int CurrentFrameNum => Decoder.CodecContext != null ? Decoder.CodecContext->frame_number : -1;

        /// <summary>
        /// MemoryStream of Audio after decoding and resampling to compatible format.
        /// </summary>
        private MemoryStream DecodedMemoryStream { get => _decodedMemoryStream; set => _decodedMemoryStream = value; }

        /// <summary>
        /// Holder of variables for Decoder
        /// </summary>
        protected FfccVariableGroup Decoder { get; set; } = new FfccVariableGroup();

        /// <summary>
        /// Based on timer and FPS determines what the current frame is.
        /// </summary>
        /// <returns>Expected Frame Number</returns>
        private int ExpectedFrame => Timer.IsRunning ? (int)Math.Round(Timer.ElapsedMilliseconds * (FPS / 1000)) : 0;

        /// <summary>
        /// FPS of the video stream.
        /// </summary>
        private unsafe double FPSVideo
        {
            get
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);

                if (Return < 0)
                {
                    return 0;
                }

                if (Decoder.Format->streams[Return]->codec->framerate.den > 0)
                {
                    return (double)Decoder.Format->streams[Return]->codec->framerate.num / Decoder.Format->streams[Return]->codec->framerate.den;
                }

                return 0;
            }
        }

        /// <summary>
        /// Mode that ffcc is running in.
        /// </summary>
        private FfccMode Mode { get; set; }

        /// <summary>
        /// Resample Context
        /// </summary>
        private unsafe SwrContext* ResampleContext { get; set; }

        /// <summary>
        /// Frame used by resampler
        /// </summary>
        private unsafe AVFrame* ResampleFrame { get; set; }

        /// <summary>
        /// Most ffmpeg functions return an integer. If the value is less than 0 it is an error
        /// usually. Sometimes data is passed and then it will be greater than 0.
        /// </summary>
        private int Return { get; set; }

        /// <summary>
        /// SWS Context
        /// </summary>
        private unsafe SwsContext* ScalerContext { get; set; }

        /// <summary>
        /// State ffcc is in.
        /// </summary>
        private FfccState State { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Dispose of all leaky variables.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Attempts to get 1 frame of Video, or refill Audio buffer.
        /// </summary>
        /// <returns>Returns -1 if missing stream or returns AVERROR or returns 0 if no problem.</returns>
        public int Next()
        {
            //if stream doesn't exist or stream is done, end
            if (Decoder.StreamIndex == -1 || State == FfccState.Done)
            {
                return -1;
            }
            // read next frame(s)

            return Update(FfccState.ReadOne);
        }

        /// <summary>
        /// Start playing Sound or Start FPS timer for Video
        /// </summary>
        /// <param name="volume">
        /// Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled
        /// by SoundEffect.MasterVolume.
        /// </param>
        /// <param name="pitch">
        /// Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).
        /// </param>
        /// <param name="pan">
        /// Panning, ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).
        /// </param>
        public void Play(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f) // there are some videos without sound meh.
        {
            if (Decoder != null && Decoder.StreamIndex > -1)
            {
                if (!Timer.IsRunning && Mode == FfccMode.StateMach && MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    Timer.Start();
                }
                if (!_useNAudio)
                {
                    if (DynamicSound != null && !DynamicSound.IsDisposed && AudioEnabled)
                    {
                        lock (_dynamicSoundEffectLock)
                        {
                            DynamicSound.Volume = volume;
                            DynamicSound.Pitch = pitch;
                            DynamicSound.Pan = pan;
                            DynamicSound.Play();
                        }
                    }

                    if (SoundEffect != null && !SoundEffect.IsDisposed && AudioEnabled)
                    {
                        SoundEffectInstance.Volume = volume;
                        SoundEffectInstance.Pitch = pitch;
                        SoundEffectInstance.Pan = pan;

                        try
                        {
                            SoundEffectInstance.Play();
                        }
                        catch (Exception e)
                        {
                            if (e.GetType().Name == "SharpDXException")
                            {
                                Mode = FfccMode.NotInit;
                                State = FfccState.NoDLL;
                                SoundEffectInstance = null;
                                SoundEffect = null;
                                // if it gets here I can't extract the sound from the SoundEffect but
                                // I can turn on nAudio and next sound will work
                                _useNAudio = true;
                            }
                            else
                                e.Rethrow();
                        }
                    }
                }
#if _WINDOWS
                else if (_bufferedWaveProvider != null && _nAudioOut != null)
                {
                    _volumeSampleProvider.Volume = volume;
                    if (_panningSampleProvider != null) // panning requires mono sound so it's null if it wasn't 1 channel.
                        _panningSampleProvider.Pan = pan;
                    // i'll leave out pitch unless it's needed. there is a provider for it but sounds
                    // like it might do more than we need.
                    _nAudioOut.Play();
                }
#endif
            }
        }

        /// <summary>
        /// Same as Play but with a task. Thread is terminated on Stop() or Dispose().
        /// </summary>
        public void PlayInTask(float volume = 1.0f, float pitch = 0.0f, float pan = 0.0f)
        {
            if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (_sourceToken == null)
                    _sourceToken = new CancellationTokenSource();
                Play(volume, pitch, pan);
                _task = new Task<int>(NextInTask);
                _task.Start();
            }
            else
                Play(volume, pitch, pan);
        }

        /// <summary>
        /// Stop playing Sound or Stop the FPS timer for Video , and Dispose of Variables
        /// </summary>
        public async void Stop()
        {
            if (Timer.IsRunning)
            {
                Timer.Stop();
                Timer.Reset();
            }
            if (!_useNAudio)
            {
                if (DynamicSound != null && !DynamicSound.IsDisposed)
                {
                    lock (_dynamicSoundEffectLock)
                    {
                        if (AudioEnabled)
                        {
                            try
                            {
                                DynamicSound?.Stop();
                            }
                            catch (NullReferenceException)
                            {
                            }
                        }

                        DynamicSound.Dispose();
                    }
                    DynamicSound = null;
                }
                if (SoundEffectInstance != null && !SoundEffectInstance.IsDisposed)
                {
                    if (AudioEnabled)
                    {
                        SoundEffectInstance.Stop();
                    }
                    SoundEffectInstance.Dispose();
                }
                if (SoundEffect != null && !SoundEffect.IsDisposed)
                {
                    SoundEffect.Dispose();
                }
            }
#if _WINDOWS
            else if (_bufferedWaveProvider != null && _nAudioOut != null)
            {
                _nAudioOut.Stop();
                try
                {
                    _nAudioOut.Dispose();
                    _bufferedWaveProvider.ClearBuffer();
                }
                catch (InvalidOperationException)
                {
                    // naudio can't be disposed of if not in original thread.
                }
            }
#endif
            if (_task != null)
            {
                _sourceToken.Cancel();
                await _task;
            }
        }

        /// <summary>
        /// Converts Frame to Texture with correct colors pace
        /// </summary>
        /// <returns>Texture2D</returns>
        public unsafe Texture2D Texture2D()
        {
            lock (Decoder)
            {
                Texture2D frameTex = new Texture2D(Memory.spriteBatch.GraphicsDevice, Decoder.CodecContext->width, Decoder.CodecContext->height, false, SurfaceFormat.Color);
                const int bpp = 4;
                byte[] texBuffer = new byte[Decoder.CodecContext->width * Decoder.CodecContext->height * bpp];
                fixed (byte* ptr = &texBuffer[0])
                {
                    byte*[] srcData = { ptr, null, null, null };
                    int[] srcLineSize = { Decoder.CodecContext->width * bpp, 0, 0, 0 };
                    // convert video frame to the RGB data
                    ffmpeg.sws_scale(ScalerContext, Decoder.Frame->data, Decoder.Frame->linesize, 0, Decoder.CodecContext->height, srcData, srcLineSize);
                }
                frameTex.SetData(texBuffer);
                return frameTex;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Decoder != null)
            {
                lock (Decoder)
                { dis(); }
            }
            else dis();

            void dis()
            {
                if (disposing)
                {
                    Stop();
                }
                if (!IsDisposed)
                {
                    if (_task != null)
                    {
                        if (_task.IsCompleted)

                            _task.Dispose();
                        else
                            Memory.FfccLeftOverTask.Add(_task);
                        _task = null;
                    }
                    State = FfccState.Done;
                    Mode = FfccMode.NotInit;
                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.
                    DecodedMemoryStream?.Dispose();
                    if (ConvertedData != null)
                    {
                        //Marshal.FreeHGlobal((IntPtr)ConvertedData);
                    }
                    //if (_intPtr != null)
                    //{
                    //    Marshal.FreeHGlobal(_intPtr);
                    //}
                    unsafe
                    {
                        ffmpeg.sws_freeContext(ScalerContext);
                        if (ResampleContext != null)
                        {
                            ffmpeg.swr_close(ResampleContext);
                            SwrContext* pResampleContext = ResampleContext;
                            ffmpeg.swr_free(&pResampleContext);
                        }
                        ffmpeg.av_frame_unref(ResampleFrame);
                        ffmpeg.av_free(ResampleFrame);
                        if (_avioCtx != null)
                        {
                            //ffmpeg.avio_close(avio_ctx); //CTD
                            ffmpeg.av_free(_avioCtx);
                        }
                    }
                    //if (avio_ctx_buffer != null)
                    //    ffmpeg.av_freep(avio_ctx_buffer); //throws exception

                    // set to true to prevent multiple disposing
                    IsDisposed = true;
                    //GC.Collect(); // don't know if this really does much. was trying to make sure the memory i'm watching is what is really there.
                }
                else
                {
#if _WINDOWS
                    if (_nAudioOut != null && _useNAudio)
                    {
                        _nAudioOut.Dispose();
                        _nAudioOut = null;
                        _bufferedWaveProvider.ClearBuffer();
                    }
#endif
                    if (_sourceToken != null)
                    {
                        _sourceToken.Dispose();
                        _sourceToken = null;
                    }

                    if (Decoder == null) return;
                    Decoder.Dispose();
                    Decoder = null;
                }
            }
        }

        /// <summary>
        /// Flush the Decoder context and packet
        /// </summary>
        /// <param name="codecContext">Decoder Codec Context</param>
        /// <param name="packet">Decoder Packet</param>
        /// <returns>0 on success, less than 0 on error</returns>
        private static unsafe void DecodeFlush(ref AVCodecContext* codecContext, ref AVPacket packet)
        {
            packet.data = null;
            packet.size = 0;

            fixed (AVPacket* tmpPacket = &packet)
            {
                CheckReturn(ffmpeg.avcodec_send_packet(codecContext, tmpPacket));
            }
        }

        /// <summary>
        /// throw new exception
        /// </summary>
        /// <param name="v">string of message</param>
        private static void Die(string v) => throw new Exception(v.Trim('\0'));


        /// <summary>
        /// For reading data from a memory pointer as if it's a file.
        /// </summary>
        /// <param name="opaque">incoming data</param>
        /// <param name="buf">outgoing data</param>
        /// <param name="bufSize">outgoing buffer size</param>
        /// <returns>Total bytes read, or EOF</returns>
        private static unsafe int ReadPacket(void* opaque, byte* buf, int bufSize)
        {
            BufferData* bd = (BufferData*)opaque;

            return bd->Read(buf, bufSize);
        }

        /// <summary>
        /// Converts FFMPEG error codes into a string.
        /// </summary>
        private static unsafe string AvError(int ret)
        {
            const ulong bufferSize = 256;
            byte[] buffer = new byte[bufferSize];
            fixed (byte* ptr = &buffer[0])
            {
                ffmpeg.av_strerror(ret, ptr, bufferSize);
            }

            return System.Text.Encoding.UTF8.GetString(buffer).Trim('\0');
        }

        /// <summary>
        /// Throws exception if Ret is less than 0
        /// </summary>
        private void CheckReturn()
        {
            CheckReturn(Return);
        }
        private static void CheckReturn(int returnVal)
        {
            switch (returnVal)
            {
                case ffmpeg.AVERROR_OUTPUT_CHANGED:
                    Die($"The swr_context output ch_layout, sample_rate, sample_fmt must match out frame! {returnVal} - {AvError(returnVal)}");
                    break;

                case ffmpeg.AVERROR_INPUT_CHANGED:
                    Die($"The swr_context input ch_layout, sample_rate, sample_fmt must match in frame! {returnVal} - {AvError(returnVal)}");
                    break;

                default:
                    if (returnVal < 0)
                    {
                        Die($"{returnVal} - {AvError(returnVal)}");
                    }

                    break;
            }
        }

        /// <summary>
        /// Decode the next frame.
        /// </summary>
        /// <param name="frame">Current Decoded Frame</param>
        /// <returns>false if EOF, or true if grabbed frame</returns>
        private unsafe bool Decode(out AVFrame frame)
        {
            do
            {
                //need this extra receive frame for when decoding audio with >1 frame per packet
                Return = ffmpeg.avcodec_receive_frame(Decoder.CodecContext, Decoder.Frame);
                if (Return == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    do
                    {
                        do
                        {
                            //make sure packet is unref before getting a new one.
                            ffmpeg.av_packet_unref(Decoder.Packet);
                            //Debug.WriteLine("Getting Packet");
                            Return = ffmpeg.av_read_frame(Decoder.Format, Decoder.Packet);
                            if (Return == ffmpeg.AVERROR_EOF)
                            {
                                goto EOF;
                            }

                            CheckReturn();
                        }
                        //check for correct stream.
                        while (Decoder.Packet->stream_index != Decoder.StreamIndex);
                        Return = ffmpeg.avcodec_send_packet(Decoder.CodecContext, Decoder.Packet);
                        ffmpeg.av_packet_unref(Decoder.Packet);
                        CheckReturn();
                        Return = ffmpeg.avcodec_receive_frame(Decoder.CodecContext, Decoder.Frame);
                    }
                    while (Return == ffmpeg.AVERROR(ffmpeg.EAGAIN));
                    CheckReturn();
                }
                else if (Return == ffmpeg.AVERROR_EOF)
                {
                    goto EOF;
                }
                else
                {
                    CheckReturn();
                }
            }
            //check for frame skip, if enabled check if behind.
            while (FrameSkip && Behind);
            frame = *Decoder.Frame;
            return true;

        //end of file, check for loop and end.
        //TODO: add LoopEnd and LOOPLength support https://github.com/FFT-Hackers/vgmstream/commit/b332e5cf5cc9e3469cbbab226836083d8377ce61
        EOF:
            Loop();
            frame = *Decoder.Frame;
            return false;
        }

        /// <summary>
        /// reads the tags from metadata
        /// </summary>
        /// <param name="metadata">metadata from format or stream</param>
        private unsafe void GetTags(ref AVDictionary* metadata)
        {
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                string key = "";
                string val = "";
                for (int i = 0; tag->value[i] != 0; i++)
                {
                    val += (char)tag->value[i];
                }

                for (int i = 0; tag->key[i] != 0; i++)
                {
                    key += (char)tag->key[i];
                }

                Metadata[key.Trim().ToUpper()] = val;
                Memory.Log.WriteLine($"{key} = {val}");
                if (key.Trim().IndexOf("LoopStart", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (int.TryParse(val, out _loopStart)) continue;
                    _loopStart = 0;
                    Memory.Log.WriteLine($"Failed Parse {key} = {val}");
                }
                else if (key.Trim().IndexOf("LoopEnd", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!int.TryParse(val, out int _))
                        Memory.Log.WriteLine($"Failed Parse {key} = {val}");
                    
                }
                else if (key.Trim().IndexOf("LoopLength", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!int.TryParse(val, out int _))
                        Memory.Log.WriteLine($"Failed Parse {key} = {val}");
                }
            }
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        protected void Init(string filename, AVMediaType mediaType = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.StateMach, int loopStart = -1)
        {
            if (!Enum.IsDefined(typeof(FfccMode), mode))
                throw new InvalidEnumArgumentException(nameof(mode), (int) mode, typeof(FfccMode));
            if (!Enum.IsDefined(typeof(AVMediaType), mediaType))
                throw new InvalidEnumArgumentException(nameof(mediaType), (int) mediaType, typeof(AVMediaType));
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_PANIC);
            LoopStart = loopStart;
            State = FfccState.Open;
            Mode = mode;
            DecodedFileName = filename;
            MediaType = mediaType;
            Return = -1;
            ConvertedData = null;
            Update();
        }

        private unsafe bool InitNaudio()
        {
#if _WINDOWS
            if (Extended.IsLinux) return false;
            _bufferedWaveProvider = new BufferedWaveProvider(
                new WaveFormat(ResampleFrame->sample_rate, ResampleFrame->channels))
            {
                DiscardOnBufferOverflow = false
            };
            _volumeSampleProvider = new VolumeSampleProvider(_bufferedWaveProvider.ToSampleProvider());
            _nAudioOut = new DirectSoundOut();
            if (ResampleFrame->channels == 1)
            {
                _panningSampleProvider = new PanningSampleProvider(_volumeSampleProvider);
                _nAudioOut.Init(_panningSampleProvider);
            }
            else
                _nAudioOut.Init(_volumeSampleProvider);
            _useNAudio = true;
            return true;
#endif
        }

        /// <summary>
        /// Sets up AVFormatContext to be able from the memory buffer.
        /// </summary>
        protected unsafe void LoadFromRam(BufferData* bd)
        {
            _avioCtx = null;

            _avioCtxBufferSize = 4096;

            _avioCtxBuffer = (byte*)ffmpeg.av_malloc((ulong)_avioCtxBufferSize);
            if (_avioCtxBuffer == null)
            {
                CheckReturn(ffmpeg.AVERROR(ffmpeg.ENOMEM));
                return;
            }
            _rf = ReadPacket;
            _sf = Seek;
            _avioCtx = ffmpeg.avio_alloc_context(_avioCtxBuffer, _avioCtxBufferSize, 0, bd, _rf, null, _sf);
            if (_avioCtx == null)
            {
                CheckReturn(ffmpeg.AVERROR(ffmpeg.ENOMEM));
                return;
            }
            Decoder.Format->pb = _avioCtx;
            Open();
        }

        private static unsafe long Seek(void* opaque, long offset, int whence)
        {
            BufferData* bd = (BufferData*)opaque;

            return bd->Seek(offset, whence);
        }

        /// <summary>
        /// Load sound from MemoryStream into a SoundEffect
        /// </summary>
        /// <param name="decodedStream">Memory Stream containing sound data</param>
        private unsafe void LoadSoundFromStream(ref MemoryStream decodedStream)
        {
            if (DecodedMemoryStream.Length <= 0 || MediaType != AVMediaType.AVMEDIA_TYPE_AUDIO) return;
            if (!_useNAudio)
            {
                if (SoundEffect != null) return;
                SoundEffect = new SoundEffect(decodedStream.GetBuffer(), 0, (int) decodedStream.Length,
                    ResampleFrame->sample_rate, (AudioChannels) ResampleFrame->channels, 0, 0);
                SoundEffectInstance = SoundEffect.CreateInstance();
                if (LoopStart >= 0)
                {
                    SoundEffectInstance.IsLooped = true;
                }
                //doesn't throw an exception till you goto play it.
            }
            else
                RecorderOnDataAvailable(new WaveInEventArgs(decodedStream.GetBuffer(), (int)decodedStream.Length));
        }

        /// <summary>
        /// Copies byte[] data to a Pointer. So it can be used with ffmpeg.
        /// </summary>
        /// <param name="data">incoming data</param>
        /// <param name="length">size of data</param>
        //private void LoadFromRAM(byte[] data, int length)
        //{
        //    _intPtr = Marshal.AllocHGlobal(length);
        //    Marshal.Copy(data, 0, _intPtr, length);
        //    LoadFromRAM(_intPtr, length);
        //}
        /// <summary>
        /// Add sound from byte[] to a Dynamic Sound Effect Instance, it can play while you give it more data.
        /// </summary>
        /// <param name="decodedStream">sound data</param>
        private unsafe void LoadSoundFromStream(ref byte[] buffer, int start, ref int length)
        {
            if (length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (!_useNAudio)
                {
                    if (DynamicSound == null)
                    {
                        //create instance here to set sample_rate and channels dynamically
                        DynamicSound = new DynamicSoundEffectInstance(ResampleFrame->sample_rate, (AudioChannels)ResampleFrame->channels);
                    }
                    try
                    {
                        lock (_dynamicSoundEffectLock)
                            DynamicSound.SubmitBuffer(buffer, start, length);
                    }
                    catch (ArgumentException)
                    {
                        //got error saying buffer was too small. makes no sense.
                    }
                    catch (Exception e)
                    {
                        if (e.GetType().Name == "SharpDXException")
                        {
                            //DynamicSound.Dispose();
                            DynamicSound = null;
                            if (!InitNaudio())
                            {
                                Mode = FfccMode.NotInit;
                                State = FfccState.NoDLL;
                            }
                            else
                            {
                                LoadSoundFromStream(ref buffer, start, ref length);
                            }
                        }
                        else
                            e.Rethrow();
                    }
                }
                else
                    RecorderOnDataAvailable(new WaveInEventArgs(start > 0 ? buffer.Skip(start).ToArray() : buffer, length - start));
            }
        }

        /// <summary>
        /// Load sound from memory stream by default.
        /// </summary>
        private void LoadSoundFromStream() => LoadSoundFromStream(ref _decodedMemoryStream);

        /// <summary>
        /// If looping seek back to LoopStart
        /// </summary>
        private unsafe void Loop()
        {
            if (LoopStart < 0 || Mode != FfccMode.StateMach) return;
            //I didn't realize this didn't change the frame number to 0. it just appends the LoopStart pos to the current stream.
            //So it is possible this could overflow when it's looped long enough to bring the current frame num to max value.
            //Return = ffmpeg.avformat_seek_file(Decoder.Format, Decoder.StreamIndex, min, LoopStart, max, 0);
            CheckReturn(ffmpeg.av_seek_frame(Decoder.Format, Decoder.StreamIndex, LoopStart, 0));
            State = FfccState.Waiting;
        }

        /// <summary>
        /// For use in threads runs Next till done. To keep audio buffer fed. Or really good timing
        /// on video frames.
        /// </summary>
        private int NextInTask()
        {
            try
            {
                while (Mode == FfccMode.StateMach && !_cancellationToken.IsCancellationRequested && State != FfccState.Done && !IsDisposed)
                {
                    lock (Decoder) //make the main thread wait if it accesses this class.
                    {
                        NextLoop();
                    }
                    if (!_useNAudio)
                        Thread.Sleep(NextAsyncSleep); //delay checks
                }
            }
            //catch (ThreadAbortException)
            //{
            //    disposeAll = true;//stop playing
            //}
            finally
            {
                Dispose(_cancellationToken.IsCancellationRequested); // dispose of everything except audio encase it's still playing.
            }
#if _WINDOWS
            if (_useNAudio)
            {
                while (!_cancellationToken.IsCancellationRequested && _nAudioOut != null && _nAudioOut.PlaybackState != PlaybackState.Stopped)
                    Thread.Sleep(NextAsyncSleep);
                //try
                //{
                //    if (nAudioOut != null)
                //        nAudioOut.Dispose();
                //    bufferedWaveProvider.ClearBuffer();
                //}
                //catch (InvalidOperationException)
                ////{
                //    if (nAudioOut != null)
                //        Memory.MainThreadOnlyActions.Enqueue(nAudioOut.Dispose);
                //    Memory.MainThreadOnlyActions.Enqueue(bufferedWaveProvider.ClearBuffer);
                //    //doesn't like threads...
                //}
            }
#endif
            return 0;
        }

        public void NextLoop()
        {
            while (!IsDisposed && !Ahead)
            {
                if (Next() < 0)
                    break;
            }
        }

        /// <summary>
        /// Opens filename and assigns FormatContext.
        /// </summary>
        private unsafe int Open()
        {
            if (!FileOpened)
            {
                fixed (AVFormatContext** tmp = &Decoder.Format)
                {
                    Return = ffmpeg.avformat_open_input(tmp, DecodedFileName, null, null);
                    CheckReturn();
                }

                Return = ffmpeg.avformat_find_stream_info(Decoder.Format, null);
                CheckReturn();

                GetTags(ref Decoder.Format->metadata);

                FileOpened = true;
            }
            return (FileOpened) ? 0 : -1;
        }

        /// <summary>
        /// Finds the codec for the chosen stream
        /// </summary>
        private unsafe void PrepareCodec()
        {
            // find & open codec
            fixed (AVCodec** tmp = &Decoder.Codec)
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, MediaType, -1, -1, tmp, 0);
            }
            if (Return == ffmpeg.AVERROR_STREAM_NOT_FOUND && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                //Don't fail if no audio stream just be done.
                State = FfccState.Done;
                Mode = FfccMode.NotInit;
                return;
            }

            CheckReturn();

            Decoder.StreamIndex = Return;
            GetTags(ref Decoder.Stream->metadata);
            Decoder.CodecContext = ffmpeg.avcodec_alloc_context3(Decoder.Codec);
            if (Decoder.CodecContext == null)
            {
                Die("Could not allocate codec context");
            }

            Return = ffmpeg.avcodec_parameters_to_context(Decoder.CodecContext, Decoder.Stream->codecpar);
            CheckReturn();
            fixed (AVDictionary** tmp = &_dict)
            {
                Return = ffmpeg.av_dict_set(tmp, "strict", "+experimental", 0);
                CheckReturn();
                Return = ffmpeg.avcodec_open2(Decoder.CodecContext, Decoder.Codec, tmp);
                CheckReturn();
            }

            if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (Decoder.CodecContext->channel_layout == 0)
                {
                    if (Decoder.CodecContext->channels == 2)
                    {
                        Decoder.CodecContext->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
                    }
                    else if (Decoder.CodecContext->channels == 1)
                    {
                        Decoder.CodecContext->channel_layout = ffmpeg.AV_CH_LAYOUT_MONO;
                    }
                    else
                    {
                        Die("must set custom channel layout, is not stereo or mono");
                    }
                }
            }
        }

        private void PrepareProcess()
        {
            using (DecodedMemoryStream = new MemoryStream())
            {
                Process();
                LoadSoundFromStream();
            }
        }

        /// <summary>
        /// PrepareResampler
        /// </summary>
        private unsafe void PrepareResampler()
        {
            if (MediaType != AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                return;
            }

            //resampler

            ResampleFrame = ffmpeg.av_frame_alloc();
            if (ResampleFrame == null)
            {
                Die("Error allocating the frame\n");
            }
            ResampleContext = ffmpeg.swr_alloc();
            ffmpeg.av_opt_set_channel_layout(ResampleContext, "in_channel_layout", (long)Decoder.CodecContext->channel_layout, 0);
            ffmpeg.av_opt_set_int(ResampleContext, "in_sample_rate", Decoder.CodecContext->sample_rate, 0);
            ffmpeg.av_opt_set_sample_fmt(ResampleContext, "in_sample_fmt", Decoder.CodecContext->sample_fmt, 0);

            ffmpeg.av_opt_set_channel_layout(ResampleContext, "out_channel_layout", (long)Decoder.CodecContext->channel_layout, 0);
            ffmpeg.av_opt_set_sample_fmt(ResampleContext, "out_sample_fmt", AVSampleFormat.AV_SAMPLE_FMT_S16, 0);
            ffmpeg.av_opt_set_int(ResampleContext, "out_sample_rate", Decoder.CodecContext->sample_rate, 0);

            Return = ffmpeg.swr_init(ResampleContext);
            if (Return < 0)
            {
                Die("swr_init");
            }
            Decoder.Frame->format = (int)Decoder.CodecContext->sample_fmt;
            Decoder.Frame->channel_layout = Decoder.CodecContext->channel_layout;
            Decoder.Frame->channels = Decoder.CodecContext->channels;
            Decoder.Frame->sample_rate = Decoder.CodecContext->sample_rate;

            ResampleFrame->nb_samples = Decoder.CodecContext->frame_size;
            if (ResampleFrame->nb_samples == 0)
            {
                ResampleFrame->nb_samples = 32; //32, or 64, ADPCM require 32.
            }

            ResampleFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
            ResampleFrame->channel_layout = Decoder.CodecContext->channel_layout;
            ResampleFrame->channels = Decoder.CodecContext->channels;
            ResampleFrame->sample_rate = Decoder.CodecContext->sample_rate;

            int convertedFrameBufferSize = ffmpeg.av_samples_get_buffer_size(null, ResampleFrame->channels,
                                                 ResampleFrame->nb_samples,
                                                 (AVSampleFormat)ResampleFrame->format, 0);

            //ConvertedData = (byte*)Marshal.AllocHGlobal(convertedFrameBufferSize);
            ConvertedData = new byte[convertedFrameBufferSize];

            if (_useNAudio) InitNaudio();
        }

        /// <summary>
        /// Setup scaler for drawing frames to bitmap.
        /// </summary>
        private unsafe void PrepareScaler()
        {
            if (MediaType != AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                return;
            }

            ScalerContext = ffmpeg.sws_getContext(
                Decoder.CodecContext->width, Decoder.CodecContext->height, Decoder.CodecContext->pix_fmt,
                Decoder.CodecContext->width, Decoder.CodecContext->height, AVPixelFormat.AV_PIX_FMT_RGBA,
                ffmpeg.SWS_ACCURATE_RND, null, null, null);
            Return = ffmpeg.sws_init_context(ScalerContext, null, null);

            CheckReturn();
        }

        /// <summary>
        /// Decodes, Resamples, Encodes
        /// </summary>
        /// <ref>https://stackoverflow.com/questions/32051847/c-ffmpeg-distorted-sound-when-converting-audio?rq=1#_=_</ref>
        private unsafe void Process()
        {
            while (Decode(out AVFrame decodedFrame))
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    if (Mode == FfccMode.StateMach)
                    {
                        State = FfccState.Waiting;
                        break;
                    }
                    // do something with video here.

                    if (Mode == FfccMode.ProcessAll)
                    {
                    }
                }
                else if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    Resample(ref decodedFrame);
                    if (Mode == FfccMode.StateMach && !Behind) //still behind stay here.
                    {
                        State = FfccState.Waiting;
                        break;
                    }
                }
            }
            if (State != FfccState.Waiting)
            {
                State = FfccState.Done;
                Timer.Stop();
                Timer.Reset();

                DecodeFlush(ref Decoder.CodecContext, ref *Decoder.Packet); //calling this twice was causing issues.
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="waveInEventArgs"></param>
        /// <see cref="https://markheath.net/post/how-to-record-and-play-audio-at-same"/>
        private void RecorderOnDataAvailable(WaveInEventArgs waveInEventArgs)
        {
#if _WINDOWS
            if (_bufferedWaveProvider == null)
                InitNaudio();

            if (_useNAudio)
                _bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
#endif
        }

        /// <summary>
        /// Resample current frame, Save output data
        /// </summary>
        /// <param name="frame">Current Decoded Frame</param>
        private unsafe void Resample(ref AVFrame frame)
        {
            // Convert
            int outSamples;
            fixed (byte** tmp = (byte*[])frame.data)
            {
                outSamples = ffmpeg.swr_convert(ResampleContext, null, 0, tmp, frame.nb_samples);
            }
            if (outSamples < 0)
            {
                Die("Could not convert");
            }
            for (; ; )
            {
                // ReSharper disable once CommentTypo
                //https://ffmpeg.org/doxygen/3.2/group__lswr.html#ga97a8d5f6abe3bcdfb6072412f17285a4
                CheckReturn(outSamples = ffmpeg.swr_get_out_samples(ResampleContext, 0));
                // 32 was nb_samples but if too big would just not decode anything
                if (outSamples < 32 * ResampleFrame->channels)
                {
                    break;
                }
                //fixed (byte** tmp = &_convertedData)
                fixed (byte* tmp = &_convertedData[0])
                {
                    //https://ffmpeg.org/doxygen/3.2/group__lswr.html#gaa5bb6cab830146efa8c760fa66ee582a
                    CheckReturn(ffmpeg.swr_convert(ResampleContext, &tmp, ResampleFrame->nb_samples, null, 0));
                }
                int bufferSize = ffmpeg.av_samples_get_buffer_size(null,
                    ResampleFrame->channels,
                    ResampleFrame->nb_samples,
                    (AVSampleFormat)ResampleFrame->format,
                    0);

                // write to buffer
                WriteToMs(ref _convertedData, 0, ref bufferSize);
            }
        }

        /// <summary>
        /// Run code depending on state
        /// </summary>
        /// <param name="state">Change the state to this</param>
        /// <param name="ret">return this</param>
        /// <returns>ret</returns>
        private int Update(FfccState state = FfccState.Null, int ret = 0)
        {
            if (Mode == FfccMode.NotInit)
            {
                Die("Class not Init");
            }

            if (state == FfccState.NoDLL)
            {
                return -1;
            }

            if (state != FfccState.Null)
            {
                State = state;
            }

            do
            {
                switch (State)
                {
                    case FfccState.Open:
                        State = FfccState.GetCodec;
                        if (0 < Open())
                        {
                            Die("Failed to Open");
                        }
                        break;

                    case FfccState.GetCodec:
                        State = FfccState.PrepareResampler;
                        PrepareCodec();
                        break;

                    case FfccState.PrepareResampler:
                        State = FfccState.PrepareScaler;
                        PrepareResampler();
                        break;

                    case FfccState.PrepareScaler:
                        State = FfccState.Read;
                        PrepareScaler();
                        break;

                    case FfccState.Read://Enters waiting state unless we want to process all now.
                        State = Mode == FfccMode.ProcessAll ? FfccState.ReadAll : FfccState.ReadOne;
                        //ReadOne here makes it grab one video frame and pre-caches audio to it's limit.
                        //Waiting here makes it wait till GetFrame() is called.
                        //ReadAll just processes the whole audio stream (memory leaky), not meant for video
                        //Currently video will just saves all the frames to bmp in temp folder on ReadAll.
                        break;

                    case FfccState.ReadAll:
                        State = FfccState.Done;
                        PrepareProcess();
                        break;

                    case FfccState.ReadOne:
                        PrepareProcess();
                        switch (State)
                        {
                            case FfccState.Waiting:
                                ret = 0;
                                break;

                            default:
                                ret = -1;
                                break;
                        }
                        break;

                    default:
                        State = FfccState.Done;
                        break;
                }
            }
            while (!((Mode == FfccMode.ProcessAll && State == FfccState.Done) || (State == FfccState.Done || State == FfccState.Waiting)));
            return ret;
        }

        private void WriteToMs(ref byte[] output, int start, ref int length)
        {
            if (Mode == FfccMode.StateMach)
            {
                LoadSoundFromStream(ref output, start, ref length);
            }
            else
            {
                DecodedMemoryStream.Write(output, start, length);
            }
        }

        #endregion Methods

    }
}