namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Graphics;
    using NAudio.Wave;
    using NAudio.Wave.SampleProviders;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Ffcc is a front end for processing Audio and Video using ffmpeg.autogen
    /// </summary>
    /// <remarks>
    /// Code bits mostly converted to c# from c++ It uses bits of code from FFmpeg examples, Aforge,
    /// FFmpeg.autogen, stackoverflow
    /// </remarks>
    public abstract partial class Ffcc : IDisposable
    {

        protected static unsafe T Load<T>(BufferData* buffer_Data, byte[] headerData, int loopstart, FfccMode ffccMode, AVMediaType avtype) where T : Ffcc, new()
        {
            T r = new T();

            void play(BufferData* d)
            {
                r.LoadFromRAM(d);
                r.Init(null, avtype, ffccMode, loopstart);
                if (ffccMode == FfccMode.PROCESS_ALL)
                {
                    ffmpeg.avformat_free_context(r.Decoder.Format);
                    //ffmpeg.avio_context_free(&Decoder._format->pb); //CTD
                    r.Decoder.Format = null;
                    r.Dispose(false);
                }
            }
            if (headerData != null)
                fixed (byte* tmp = &headerData[0])
                {
                    lock (r.Decoder)
                    {
                        buffer_Data->SetHeader(tmp);
                        play(buffer_Data);
                    }
                }
            else
                play(buffer_Data);
            return r;
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        protected static T Load<T>(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH, int loopstart = -1) where T : Ffcc, new()
        {
            T r = new T();
            r.Init(filename, mediatype, mode, loopstart);
            if (mode == FfccMode.PROCESS_ALL)
                r.Dispose(false);
            return r;
        }

        #region Fields

        /// <summary>
        /// If you have sound skipping increase this number and it'll go away. might decrease sync or
        /// increase memory load The goal is to keep the dynamicsoundeffectinterface fed. If it plays
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
        private static bool useNaudio = false;

        private object DynamicSoundEffectLock = new object();
        private readonly unsafe AVDictionary* _dict;
        private unsafe AVIOContext* _avio_ctx;
        private unsafe byte* _avio_ctx_buffer;
        private int _avio_ctx_buffer_size;
        private byte[] _convertedData;

        //private byte* _convertedData;
        private MemoryStream _decodedMemoryStream;

        private bool _frameSkip = true;

        //private IntPtr _intPtr;
        private int _loopstart;

        /// <summary>
        /// buffered wave provider used to handle audio samples
        /// </summary>
        /// <see cref="https://markheath.net/post/how-to-record-and-play-audio-at-same"/>

        private CancellationToken cancellationToken;
#if _WINDOWS
        private BufferedWaveProvider bufferedWaveProvider;

        /// <summary>
        /// Wave out for naudio only works in windows.
        /// </summary>
        /// <see cref="https://markheath.net/post/how-to-record-and-play-audio-at-same"/>
        private DirectSoundOut nAudioOut;

        /// <summary>
        /// Directsound requires VolumeSampleProvider to change volume.
        /// </summary>
        private VolumeSampleProvider volumeSampleProvider;

        /// <summary>
        /// Required by naudio to pan the sound.
        /// </summary>
        private PanningSampleProvider panningSampleProvider;

#endif
        private avio_alloc_context_read_packet rf;
        private avio_alloc_context_seek sf;
        private CancellationTokenSource sourceToken;

        private bool stopped = false;

        private Task task;
        private FfccVaribleGroup _decoder = new FfccVaribleGroup();
        private DynamicSoundEffectInstance _dynamicSound;
        private int _loopend;
        private int _looplength;

        #endregion Fields

        #region Destructors

        ~Ffcc()
        {
            Dispose(false);
        }

        #endregion Destructors

        #region Enums

        //public FileStream DecodeFileStream { get => _decodeFileStream; set => _decodeFileStream = value; }
        public enum FfccMode
        {
            /// <summary>
            /// Processes entire file at once and does something with output
            /// </summary>
            PROCESS_ALL,

            /// <summary>
            /// State machine, functions in this call update to update current state. And update
            /// decides what to do next.
            /// </summary>
            STATE_MACH,

            /// <summary>
            /// Not Init some error happened that prevented the class from working
            /// </summary>
            NOTINIT
        }

        public enum FfccState
        {
            OPEN,

            /// <summary>
            /// Waiting for request for next frame
            /// </summary>
            WAITING,

            /// <summary>
            /// Readall the data
            /// </summary>
            READALL,

            /// <summary>
            /// Done reading file nothing more can be done
            /// </summary>
            DONE,

            /// <summary>
            /// Don't change state just pass ret value.
            /// </summary>
            NULL,

            /// <summary>
            /// Get packet of data containing frames
            /// </summary>
            READONE,

            /// <summary>
            /// Missing DLL required to function
            /// </summary>
            NODLL,

            /// <summary>
            /// Gets stream and Codec
            /// </summary>
            GETCODEC,

            /// <summary>
            /// Prepares Scaler for Video stream
            /// </summary>
            PREPARE_SWS,

            /// <summary>
            /// Start Reading
            /// </summary>
            READ,

            /// <summary>
            /// Prepares Resampler for Audio stream
            /// </summary>
            PREPARE_SWR
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
                if (Decoder.StreamIndex != -1 && Mode == FfccMode.STATE_MACH)
                {
                    if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                    {
                        if (DynamicSound != null)
                        {
                            return DynamicSound.PendingBufferCount > GoalBufferCount;
                        }
#if _WINDOWS
                        else if (useNaudio)
                        {
                            return bufferedWaveProvider.BufferedDuration.TotalSeconds > bufferedWaveProvider.BufferDuration.TotalSeconds - 1;
                        }
#endif
                    }
                    else if (timer.IsRunning)
                    {
                        return CurrentFrameNum > ExpectedFrame;
                    }
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
                if (Decoder.StreamIndex != -1 && Mode == FfccMode.STATE_MACH)
                {
                    if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                    {
                        if (DynamicSound != null)
                        {
                            return DynamicSound.PendingBufferCount == GoalBufferCount;
                        }
#if _WINDOWS
                        else if (useNaudio)
                        {
                            return bufferedWaveProvider.BufferedDuration.TotalSeconds == bufferedWaveProvider.BufferDuration.TotalSeconds - 1;
                        }
#endif
                        else
                        {
                            die($"{Decoder.CodecContext->sample_rate} is currently unsupported");
                        }
                    }
                    else if (timer.IsRunning)
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
        public string DecodedFileName { get; private set; }

        /// <summary>
        /// Dynamic Sound Effect Interface for class allows control out of class. Mode must be in STATE_MACH
        /// </summary>
        public DynamicSoundEffectInstance DynamicSound
        {
            get => _dynamicSound; private set
            {
                lock (DynamicSoundEffectLock)
                    _dynamicSound = value;
            }
        }

        /// <summary>
        /// True if file is open.
        /// </summary>
        public bool FileOpened { get; private set; }

        /// <summary>
        /// returns Frames per second or if that is 0. it will return the Time_Base ratio. This is
        /// the fundamental unit of time (in seconds) in terms of which frame timestamps are
        /// represented. In many cases the audio files time base is the same as the sample rate.
        /// example 1/44100. video files audio stream might be 1/100 or 1/1000. these can make for
        /// large durrations.
        /// </summary>
        public unsafe double FPS
        {
            get
            {
                double r = FPSvideo;
                if (r != 0)
                {
                    return r;
                }
                else
                {
                    if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO && Decoder.CodecContext != null && Decoder.CodecContext->framerate.den != 0)
                    {
                        return Decoder.CodecContext->framerate.num / (double)Decoder.CodecContext->framerate.den;
                    }
                    else if (Decoder.Stream != null && Decoder.Stream->time_base.den != 0)
                    {
                        return Decoder.Stream->time_base.num / (double)Decoder.Stream->time_base.den; // returns the time_base
                    }
                }

                return 0;
            }
        }

        /// <summary>
        /// When getting video frames if behind it goes to next frame. disabled for debugging purposes.
        /// </summary>
        public bool FrameSkip { get => MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO ? _frameSkip : false; set => _frameSkip = value; }

        /// <summary>
        /// Is the class disposed of. If true calling Dispose() does nothing.
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// Sample Count that loop starts from.
        /// </summary>
        public int LOOPSTART { get => _loopstart; set => _loopstart = value; }

        /// <summary>
        /// Current media type being processed.
        /// </summary>
        public AVMediaType MediaType { get; private set; }

        /// <summary>
        /// Metadata container for tags.
        /// </summary>
        public Dictionary<String, String> Metadata { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// SoundEffect for class allows control out of class. Mode must be in PROCESS_ALL
        /// </summary>
        public SoundEffect SoundEffect { get; private set; }

        /// <summary>
        /// SoundEffectInterface for class. allows for more control than just playing the above soundeffect.
        /// </summary>
        public SoundEffectInstance SoundEffectInstance { get; private set; }

        /// <summary>
        /// Stopwatch tracks the time audio has played so video can sync or loops can be looped.
        /// </summary>
        public Stopwatch timer { get; } = new Stopwatch();

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
        /// MemoryStream of Audio after decoding and resamping to compatable format.
        /// </summary>
        private MemoryStream DecodedMemoryStream { get => _decodedMemoryStream; set => _decodedMemoryStream = value; }

        /// <summary>
        /// Holder of varibles for Decoder
        /// </summary>
        protected FfccVaribleGroup Decoder { get => _decoder; set => _decoder = value; }

        /// <summary>
        /// Based on timer and FPS determines what the current frame is.
        /// </summary>
        /// <returns>Expected Frame Number</returns>
        private int ExpectedFrame => timer.IsRunning ? (int)Math.Round(timer.ElapsedMilliseconds * (FPS / 1000)) : 0;

        /// <summary>
        /// FPS of the video stream.
        /// </summary>
        private unsafe double FPSvideo
        {
            get
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, AVMediaType.AVMEDIA_TYPE_VIDEO, -1, -1, null, 0);

                if (Return < 0)
                {
                    return 0;
                }
                else if (Decoder.Format->streams[Return]->codec->framerate.den > 0)
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
        /// Dispose of all leaky varibles.
        /// </summary>
        public void Dispose() => Dispose(true);

        /// <summary>
        /// Attempts to get 1 frame of Video, or refill Audio buffer.
        /// </summary>
        /// <returns>Returns -1 if missing stream or returns AVERROR or returns 0 if no problem.</returns>
        public int Next()
        {
            //if stream doesn't exist or stream is done, end
            if (Decoder.StreamIndex == -1 || State == FfccState.DONE)
            {
                return -1;
            }
            // read next frame(s)
            else
            {
                return Update(FfccState.READONE);
            }
        }

        /// <summary>
        /// Pause or Resume timer. WIP
        /// </summary>
        public void Pause()
        {
            if (Decoder.StreamIndex > -1)
            {
                if (Mode == FfccMode.STATE_MACH)
                {
                    if (!timer.IsRunning)
                    {
                        timer.Stop();
                    }
                    else
                    {
                        timer.Start();
                    }
                }
            }
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
                if (!timer.IsRunning && Mode == FfccMode.STATE_MACH && MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    timer.Start();
                }
                if (!useNaudio)
                {
                    if (DynamicSound != null && !DynamicSound.IsDisposed && AudioEnabled)
                    {
                        lock (DynamicSoundEffectLock)
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
                                Mode = FfccMode.NOTINIT;
                                State = FfccState.NODLL;
                                SoundEffectInstance = null;
                                SoundEffect = null;
                                // if it gets here I can't extract the sound from the SoundEffect but
                                // I can turn on nAudio and next sound will work
                                useNaudio = true;
                            }
                            else
                                e.Rethrow();
                        }
                    }
                }
#if _WINDOWS
                else if (bufferedWaveProvider != null && nAudioOut != null)
                {
                    volumeSampleProvider.Volume = volume;
                    if (panningSampleProvider != null) // panning requires mono sound so it's null if it wasn't 1 channel.
                        panningSampleProvider.Pan = pan;
                    // i'll leave out pitch unless it's needed. there is a provider for it but sounds
                    // like it might do more than we need.
                    nAudioOut.Play();
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
                if (sourceToken == null)
                    sourceToken = new CancellationTokenSource();
                if (cancellationToken == null)
                    cancellationToken = sourceToken.Token;
                Play(volume, pitch, pan);
                task = new Task<int>(NextinTask);
                task.Start();
            }
            else
                Play(volume, pitch, pan);
        }

        /// <summary>
        /// Stop playing Sound or Stop the FPS timer for Video , and Dispose of Varibles
        /// </summary>
        public async void Stop()
        {
            if (stopped)
                return;
            if (timer.IsRunning)
            {
                timer.Stop();
                timer.Reset();
            }
            if (!useNaudio)
            {
                if (DynamicSound != null && !DynamicSound.IsDisposed)
                {
                    lock (DynamicSoundEffectLock)
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
            else if (bufferedWaveProvider != null && nAudioOut != null)
            {
                nAudioOut.Stop();
                try
                {
                    nAudioOut.Dispose();
                    bufferedWaveProvider.ClearBuffer();
                }
                catch (InvalidOperationException)
                {
                    // naudio can't be disposed of if not in original thread.
                }
            }
#endif
            if (task != null)
            {
                sourceToken.Cancel();
                await task;
            }
        }

        /// <summary>
        /// Converts Frame to Texture with correct colorspace
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
                    int[] srcLinesize = { Decoder.CodecContext->width * bpp, 0, 0, 0 };
                    // convert video frame to the RGB data
                    ffmpeg.sws_scale(ScalerContext, Decoder.Frame->data, Decoder.Frame->linesize, 0, Decoder.CodecContext->height, srcData, srcLinesize);
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
                    if (task != null)
                    {
                        if (task.IsCompleted)

                            task.Dispose();
                        else
                            Memory.FfccLeftOverTask.Add(task);
                        task = null;
                    }
                    State = FfccState.DONE;
                    Mode = FfccMode.NOTINIT;
                    // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                    // TODO: set large fields to null.
                    if (DecodedMemoryStream != null)
                    {
                        DecodedMemoryStream.Dispose();
                    }
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
                        if (_avio_ctx != null)
                        {
                            //ffmpeg.avio_close(avio_ctx); //CTD
                            ffmpeg.av_free(_avio_ctx);
                        }
                    }
                    //if (avio_ctx_buffer != null)
                    //    ffmpeg.av_freep(avio_ctx_buffer); //throws exception

                    // set to true to prevent multiple disposings
                    IsDisposed = true;
                    //GC.Collect(); // donno if this really does much. was trying to make sure the memory i'm watching is what is really there.
                }
                else
                {
#if _WINDOWS
                    if (nAudioOut != null && useNaudio)
                    {
                        nAudioOut.Dispose();
                        nAudioOut = null;
                        bufferedWaveProvider.ClearBuffer();
                    }
#endif
                    if (sourceToken != null)
                    {
                        sourceToken.Dispose();
                        sourceToken = null;
                    }
                    if (_decoder != null)
                    {
                        _decoder.Dispose();
                        _decoder = null;
                    }
                }
            }
        }

        /// <summary>
        /// Flush the Decoder context and packet
        /// </summary>
        /// <param name="avctx">Decoder Codec Context</param>
        /// <param name="avpkt">Decoder Packet</param>
        /// <returns>0 on success, less than 0 on error</returns>
        private static unsafe int DecodeFlush(ref AVCodecContext* avctx, ref AVPacket avpkt)
        {
            avpkt.data = null;
            avpkt.size = 0;

            fixed (AVPacket* tmpPacket = &avpkt)
            {
                return ffmpeg.avcodec_send_packet(avctx, tmpPacket);
            }
        }

        /// <summary>
        /// throw new exception
        /// </summary>
        /// <param name="v">string of message</param>
        private static void die(string v) => throw new Exception(v.Trim('\0'));


        /// <summary>
        /// For reading data from a memory pointer as if it's a file.
        /// </summary>
        /// <param name="opaque">incoming data</param>
        /// <param name="buf">outgoing data</param>
        /// <param name="buf_size">outgoing buffer size</param>
        /// <returns>Total bytes read, or EOF</returns>
        private static unsafe int Read_packet(void* opaque, byte* buf, int buf_size)
        {
            BufferData* bd = (BufferData*)opaque;

            return bd->Read(buf, buf_size);
        }

        /// <summary>
        /// Converts FFMPEG error codes into a string.
        /// </summary>
        private unsafe string AvError(int ret)
        {
            ulong errbuff_size = 256;
            byte[] errbuff = new byte[errbuff_size];
            fixed (byte* ptr = &errbuff[0])
            {
                ffmpeg.av_strerror(ret, ptr, errbuff_size);
            }

            return System.Text.Encoding.UTF8.GetString(errbuff).Trim('\0');
        }

        /// <summary>
        /// Throws exception if Ret is less than 0
        /// </summary>
        private int CheckReturn()
        {
            switch (Return)
            {
                case ffmpeg.AVERROR_OUTPUT_CHANGED:
                    die($"The swr_context output ch_layout, sample_rate, sample_fmt must match outframe! {Return} - {AvError(Return)}");
                    break;

                case ffmpeg.AVERROR_INPUT_CHANGED:
                    die($"The swr_context input ch_layout, sample_rate, sample_fmt must match inframe! {Return} - {AvError(Return)}");
                    break;

                default:
                    if (Return < 0)
                    {
                        die($"{Return} - {AvError(Return)}");
                    }

                    break;
            }
            return Return;
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
                            else
                            {
                                CheckReturn();
                            }
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
            //check for frameskip, if enabled check if behind.
            while (FrameSkip && Behind);
            frame = *Decoder.Frame;
            return true;

        //end of file, check for loop and end.
        //TODO: add LOOPEND and LOOPLENGTH support https://github.com/FFT-Hackers/vgmstream/commit/b332e5cf5cc9e3469cbbab226836083d8377ce61
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
                Debug.WriteLine($"{key} = {val}");
                if (key.Trim().IndexOf("LOOPSTART", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!int.TryParse(val, out _loopstart))
                    {
                        _loopstart = 0;
                        Debug.WriteLine($"Failed Parse {key} = {val}");
                    }
                }
                else if (key.Trim().IndexOf("LOOPEND", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (int.TryParse(val, out _loopend))
                        _looplength = _loopend - _loopstart;
                    else
                        Debug.WriteLine($"Failed Parse {key} = {val}");
                }
                else if (key.Trim().IndexOf("LOOPLENGTH", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (int.TryParse(val, out _looplength))
                        _loopend = _loopend + _loopstart;
                    else
                        Debug.WriteLine($"Failed Parse {key} = {val}");
                }
            }
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        protected void Init(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH, int loopstart = -1)
        {
            ffmpeg.av_log_set_level(ffmpeg.AV_LOG_PANIC);
            LOOPSTART = loopstart;
            State = FfccState.OPEN;
            Mode = mode;
            DecodedFileName = filename;
            MediaType = mediatype;
            Return = -1;
            ConvertedData = null;
            Update();
        }

        private unsafe bool initNaudio()
        {
#if _WINDOWS
            if (!Extended.IsLinux)
            {
                bufferedWaveProvider = new BufferedWaveProvider(
                    new WaveFormat(ResampleFrame->sample_rate, ResampleFrame->channels))
                {
                    DiscardOnBufferOverflow = false
                };
                volumeSampleProvider = new VolumeSampleProvider(bufferedWaveProvider.ToSampleProvider());
                nAudioOut = new DirectSoundOut();
                if (ResampleFrame->channels == 1)
                {
                    panningSampleProvider = new PanningSampleProvider(volumeSampleProvider);
                    nAudioOut.Init(panningSampleProvider);
                }
                else
                    nAudioOut.Init(volumeSampleProvider);
                useNaudio = true;
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Sets up AVFormatContext to be able from the memory buffer.
        /// </summary>
        protected unsafe void LoadFromRAM(BufferData* bd)
        {
            _avio_ctx = null;

            _avio_ctx_buffer_size = 4096;
            int ret = 0;
            //_bufferData = new Buffer_Data
            //{
            //    Header = buffer,
            //    HeaderSize = buffer_size
            //};

            _avio_ctx_buffer = (byte*)ffmpeg.av_malloc((ulong)_avio_ctx_buffer_size);
            if (_avio_ctx_buffer == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
                return;
            }
            rf = new avio_alloc_context_read_packet(Read_packet);
            sf = new avio_alloc_context_seek(Seek);
            _avio_ctx = ffmpeg.avio_alloc_context(_avio_ctx_buffer, _avio_ctx_buffer_size, 0, bd, rf, null, sf);

            if (_avio_ctx == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            }
            Decoder._format->pb = _avio_ctx;
            Open();
        }

        private unsafe long Seek(void* opaque, long offset, int whence)
        {
            BufferData* bd = (BufferData*)opaque;

            return bd->Seek(offset, whence);
        }

        /// <summary>
        /// Load sound from Memorystream into a SoundEFFect
        /// </summary>
        /// <param name="decodedStream">Memory Stream containing sound data</param>
        private unsafe void LoadSoundFromStream(ref MemoryStream decodedStream)
        {
            if (DecodedMemoryStream.Length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (!useNaudio)
                {
                    if (SoundEffect == null)
                    {
                        SoundEffect = new SoundEffect(decodedStream.GetBuffer(), 0, (int)decodedStream.Length, ResampleFrame->sample_rate, (AudioChannels)ResampleFrame->channels, 0, 0);
                        SoundEffectInstance = SoundEffect.CreateInstance();
                        if (LOOPSTART >= 0)
                        {
                            SoundEffectInstance.IsLooped = true;
                        }
                        //doesn't throw an exception till you goto play it.
                    }
                }
                else
                    RecorderOnDataAvailable(this, new WaveInEventArgs(decodedStream.GetBuffer(), (int)decodedStream.Length));
            }
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
        /// Add sound from byte[] to a DynamicSoundEFFectInstance, it can play while you give it more data.
        /// </summary>
        /// <param name="decodedStream">sound data</param>
        private unsafe void LoadSoundFromStream(ref byte[] buffer, int start, ref int length)
        {
            if (length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (!useNaudio)
                {
                    if (DynamicSound == null)
                    {
                        //create instance here to set sample_rate and channels dynamicly
                        DynamicSound = new DynamicSoundEffectInstance(ResampleFrame->sample_rate, (AudioChannels)ResampleFrame->channels);
                    }
                    try
                    {
                        lock (DynamicSoundEffectLock)
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
                            if (!initNaudio())
                            {
                                Mode = FfccMode.NOTINIT;
                                State = FfccState.NODLL;
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
                    RecorderOnDataAvailable(this, new WaveInEventArgs(start > 0 ? buffer.Skip(start).ToArray() : buffer, length - start));
            }
        }

        /// <summary>
        /// Load sound from memory stream by default.
        /// </summary>
        private void LoadSoundFromStream() => LoadSoundFromStream(ref _decodedMemoryStream);

        /// <summary>
        /// If looping seek back to LOOPSTART
        /// </summary>
        private unsafe void Loop()
        {
            if (LOOPSTART >= 0 && Mode == FfccMode.STATE_MACH)
            {
                int min = LOOPSTART - 1000;
                if (min < 0)
                {
                    min = 0;
                }

                long max = Decoder.Stream->duration;
                if (max <= LOOPSTART)
                {
                    max = LOOPSTART + 1000;
                }
                //I didn't realize this didn't change the framenumber to 0. it just appends the LOOPSTART pos to the current stream.
                //So it is possible this could overflow when it's looped long enough to bring the currentframenum to max value.
                //Return = ffmpeg.avformat_seek_file(Decoder.Format, Decoder.StreamIndex, min, LOOPSTART, max, 0);
                Return = ffmpeg.av_seek_frame(Decoder.Format, Decoder.StreamIndex, LOOPSTART, 0);
                
                CheckReturn();

                State = FfccState.WAITING;
            }
        }

        /// <summary>
        /// For use in threads runs Next till done. To keep audio buffer fed. Or really good timing
        /// on video frames.
        /// </summary>
        private int NextinTask()
        {
            try
            {
                while (Mode == FfccMode.STATE_MACH && !cancellationToken.IsCancellationRequested && State != FfccState.DONE && !IsDisposed)
                {
                    lock (Decoder) //make the main thread wait if it accesses this class.
                    {
                        NextLoop();
                    }
                    if (!useNaudio)
                        Thread.Sleep(NextAsyncSleep); //delay checks
                }
            }
            //catch (ThreadAbortException)
            //{
            //    disposeAll = true;//stop playing
            //}
            finally
            {
                Dispose(cancellationToken.IsCancellationRequested); // dispose of everything except audio encase it's still playing.
            }
#if _WINDOWS
            if (useNaudio)
            {
                while (!cancellationToken.IsCancellationRequested && nAudioOut != null && nAudioOut.PlaybackState != PlaybackState.Stopped)
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
                fixed (AVFormatContext** tmp = &Decoder._format)
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
            fixed (AVCodec** tmp = &Decoder._codec)
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, MediaType, -1, -1, tmp, 0);
            }
            if (Return == ffmpeg.AVERROR_STREAM_NOT_FOUND && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                //Don't fail if no audiostream just be done.
                State = FfccState.DONE;
                Mode = FfccMode.NOTINIT;
                return;
            }
            else
            {
                CheckReturn();
            }

            Decoder.StreamIndex = Return;
            GetTags(ref Decoder.Stream->metadata);
            Decoder.CodecContext = ffmpeg.avcodec_alloc_context3(Decoder.Codec);
            if (Decoder.CodecContext == null)
            {
                die("Could not allocate codec context");
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
                        die("must set custom channel layout, is not stereo or mono");
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
                die("Error allocating the frame\n");
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
                die("swr_init");
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

            if (useNaudio) initNaudio();
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
            while (Decode(out AVFrame _DecodedFrame))
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    if (Mode == FfccMode.STATE_MACH)
                    {
                        State = FfccState.WAITING;
                        break;
                    }
                    // do something with video here.
                    else if (Mode == FfccMode.PROCESS_ALL)
                    {
                    }
                }
                else if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    Resample(ref _DecodedFrame);
                    if (Mode == FfccMode.STATE_MACH && !Behind) //still behind stay here.
                    {
                        State = FfccState.WAITING;
                        break;
                    }
                }
            }
            if (State != FfccState.WAITING)
            {
                State = FfccState.DONE;
                timer.Stop();
                timer.Reset();

                DecodeFlush(ref Decoder._codecContext, ref *Decoder.Packet); //calling this twice was causing issues.
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="waveInEventArgs"></param>
        /// <see cref="https://markheath.net/post/how-to-record-and-play-audio-at-same"/>
        private void RecorderOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
#if _WINDOWS
            if (bufferedWaveProvider == null)
                initNaudio();

            if (useNaudio)
                bufferedWaveProvider.AddSamples(waveInEventArgs.Buffer, 0, waveInEventArgs.BytesRecorded);
#endif
        }

        /// <summary>
        /// Resample current frame, Save output data
        /// </summary>
        /// <param name="frame">Current Decoded Frame</param>
        private unsafe void Resample(ref AVFrame frame)
        {
            // Convert
            int outSamples = 0;
            fixed (byte** tmp = (byte*[])frame.data)
            {
                outSamples = ffmpeg.swr_convert(ResampleContext, null, 0, tmp, frame.nb_samples);
            }
            if (outSamples < 0)
            {
                die("Could not convert");
            }
            for (; ; )
            {
                outSamples = ffmpeg.swr_get_out_samples(ResampleContext, 0);
                // 32 was nb_samples but if too big would just not decode anything
                if (outSamples < 32 * ResampleFrame->channels)
                {
                    break;
                }
                //fixed (byte** tmp = &_convertedData)
                fixed (byte* tmp = &_convertedData[0])
                {
                    outSamples = ffmpeg.swr_convert(ResampleContext, &tmp, ResampleFrame->nb_samples, null, 0);
                }
                int buffer_size = ffmpeg.av_samples_get_buffer_size(null,
                    ResampleFrame->channels,
                    ResampleFrame->nb_samples,
                    (AVSampleFormat)ResampleFrame->format,
                    0);

                // write to buffer
                WritetoMs(ref _convertedData, 0, ref buffer_size);
            }
        }

        /// <summary>
        /// Run code depending on state
        /// </summary>
        /// <param name="state">Change the state to this</param>
        /// <param name="ret">return this</param>
        /// <returns>ret</returns>
        private int Update(FfccState state = FfccState.NULL, int ret = 0)
        {
            if (Mode == FfccMode.NOTINIT)
            {
                die("Class not Init");
            }

            if (state == FfccState.NODLL)
            {
                return -1;
            }

            if (state != FfccState.NULL)
            {
                State = state;
            }

            do
            {
                switch (State)
                {
                    case FfccState.OPEN:
                        State = FfccState.GETCODEC;
                        if (0 < Open())
                        {
                            die("Failed to Open");
                        }
                        break;

                    case FfccState.GETCODEC:
                        State = FfccState.PREPARE_SWR;
                        PrepareCodec();
                        break;

                    case FfccState.PREPARE_SWR:
                        State = FfccState.PREPARE_SWS;
                        PrepareResampler();
                        break;

                    case FfccState.PREPARE_SWS:
                        State = FfccState.READ;
                        PrepareScaler();
                        break;

                    case FfccState.READ://Enters waiting state unless we want to process all now.
                        State = Mode == FfccMode.PROCESS_ALL ? FfccState.READALL : FfccState.READONE;
                        //READONE here makes it grab one video frame and precaches audio to it's limit.
                        //WAITING here makes it wait till GetFrame() is called.
                        //READALL just processes the whole audio stream (memoryleaky), not ment for video
                        //Currently video will just saves all the frames to bmp in temp folder on READALL.
                        break;

                    case FfccState.READALL:
                        State = FfccState.DONE;
                        PrepareProcess();
                        break;

                    case FfccState.READONE:
                        PrepareProcess();
                        switch (State)
                        {
                            case FfccState.WAITING:
                                ret = 0;
                                break;

                            default:
                                ret = -1;
                                break;
                        }
                        break;

                    default:
                        State = FfccState.DONE;
                        break;
                }
            }
            while (!((Mode == FfccMode.PROCESS_ALL && State == FfccState.DONE) || (State == FfccState.DONE || State == FfccState.WAITING)));
            return ret;
        }

        private int WritetoMs(ref byte[] output, int start, ref int length)
        {
            if (Mode == FfccMode.STATE_MACH)
            {
                LoadSoundFromStream(ref output, start, ref length);
            }
            else
            {
                DecodedMemoryStream.Write(output, start, length);
            }
            return length - start;
        }

        /// <summary>
        /// Write to Memory Stream.
        /// </summary>
        /// <param name="output">Byte pointer to output buffer array</param>
        /// <param name="start">Start from typically 0</param>
        /// <param name="length">Total bytes to read.</param>
        /// <returns>bytes wrote</returns>
        private unsafe int WritetoMs(ref byte* output, int start, ref int length)
        {
            if (Mode == FfccMode.STATE_MACH)
            {
                byte[] arr = new byte[length];
                Marshal.Copy((IntPtr)output, arr, 0, length);
                LoadSoundFromStream(ref arr, start, ref length);
                return length;
            }
            else
            {
                //memory leaky? seems when i used this method alot of memory wouldn't get disposed, might be only with large sounds
                long c_len = DecodedMemoryStream.Length;
                for (int i = start; i < length; i++)
                {
                    DecodedMemoryStream.WriteByte(output[i]);
                }
                if (DecodedMemoryStream.Length - c_len != length)
                {
                    die("not all data wrote");
                }

                return (int)(DecodedMemoryStream.Length - c_len);
            }
        }

#endregion Methods

    }
}