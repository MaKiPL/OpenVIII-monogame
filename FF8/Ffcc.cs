namespace FF8
{
    using FFmpeg.AutoGen;
    using Microsoft.Xna.Framework.Audio;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    /// <summary>
    /// Ffcc is a front end for processing Audio and Video using ffmpeg.autogen
    /// </summary>
    /// <remarks>
    /// Code bits mostly converted to c# from c++ It uses bits of code from FFmpeg examples, Aforge,
    /// FFmpeg.autogen, stackoverflow
    /// </remarks>
    internal unsafe class Ffcc : IDisposable
    {
        #region Fields

        /// <summary>
        /// If you have sound skipping increase this number and it'll go away. might decrease sync or
        /// increase memory load The goal is to keep the dynamicsoundeffectinterface fed. if it plays
        /// the audio before you give it more, then you get sound skips.
        /// </summary>
        /// <value>The goal buffer count.</value>
        /// <remarks>Will want to be as low as possible without sound skipping</remarks>
        private const int GoalBufferCount = 75;

        private readonly AVDictionary* _dict;
        private AVIOContext* _avio_ctx;
        private byte* _avio_ctx_buffer;
        private int _avio_ctx_buffer_size;
        private Buffer_data _bufferData;
        private byte* _convertedData;
        private MemoryStream _decodedMemoryStream;
        private bool _frameSkip = true;
        private IntPtr _intPtr;
        private int _loopstart;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public Ffcc(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH, int loopstart = -1)
        {
            Init(filename, mediatype, mode, loopstart);
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        /// <remarks>
        /// based on
        /// https://stackoverflow.com/questions/9604633/reading-a-file-located-in-memory-with-libavformat
        /// and http://www.ffmpeg.org/doxygen/trunk/doc_2examples_2avio_reading_8c-example.html and
        /// https://stackoverflow.com/questions/24758386/intptr-to-callback-function probably could
        /// be wrote better theres alot of hoops to jump threw
        /// </remarks>
        public Ffcc(byte[] data, int length, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.PROCESS_ALL, int loopstart = -1)
        {
            LoadFromRAM(data, length);
            Init(null, mediatype, mode, loopstart);
        }

        #endregion Constructors

        #region Destructors

        ~Ffcc()
        {
            Dispose();
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
        public bool Current
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
        public DynamicSoundEffectInstance DynamicSound { get; private set; }

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
        public double FPS
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
        public bool isDisposed { get; private set; } = false;

        /// <summary>
        /// Sample count that loop starts from.
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

        private byte* ConvertedData { get => _convertedData; set => _convertedData = value; }

        /// <summary>
        /// Current frame number
        /// </summary>
        /// <returns>Current frame number or -1 on error</returns>
        private int CurrentFrameNum => Decoder.CodecContext != null ? Decoder.CodecContext->frame_number : -1;

        /// <summary>
        /// MemoryStream of Audio after decoding and resamping to compatable format.
        /// </summary>
        private MemoryStream DecodedMemoryStream { get => _decodedMemoryStream; set => _decodedMemoryStream = value; }

        /// <summary>
        /// Holder of varibles for Decoder
        /// </summary>
        private FfccVaribleGroup Decoder { get; set; } = new FfccVaribleGroup();

        /// <summary>
        /// Based on timer and FPS determines what the current frame is.
        /// </summary>
        /// <returns>Expected Frame Number</returns>
        private int ExpectedFrame => timer.IsRunning ? (int)Math.Round(timer.ElapsedMilliseconds * (FPS / 1000)) : 0;

        /// <summary>
        /// FPS of the video stream.
        /// </summary>
        private double FPSvideo
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
        private SwrContext* ResampleContext { get; set; }

        /// <summary>
        /// Frame used by resampler
        /// </summary>
        private AVFrame* ResampleFrame { get; set; }

        /// <summary>
        /// Most ffmpeg functions return an integer. If the value is less than 0 it is an error
        /// usually. Sometimes data is passed and then it will be greater than 0.
        /// </summary>
        private int Return { get; set; }

        /// <summary>
        /// SWS Context
        /// </summary>
        private SwsContext* ScalerContext { get; set; }

        /// <summary>
        /// State ffcc is in.
        /// </summary>
        private FfccState State { get; set; }
        
        #endregion Properties

        #region Methods

        /// <summary>
        /// Flush the Decoder context and packet
        /// </summary>
        /// <param name="avctx">Decoder Codec Context</param>
        /// <param name="avpkt">Decoder Packet</param>
        /// <returns>0 on success, less than 0 on error</returns>
        public static int DecodeFlush(ref AVCodecContext* avctx, ref AVPacket avpkt)
        {
            avpkt.data = null;
            avpkt.size = 0;

            fixed (AVPacket* tmpPacket = &avpkt)
            {
                return ffmpeg.avcodec_send_packet(avctx, tmpPacket);
            }
        }

        /// <summary>
        /// Decode the next frame.
        /// </summary>
        /// <param name="frame">Current Decoded Frame</param>
        /// <returns>false if EOF, or true if grabbed frame</returns>
        public bool Decode(out AVFrame frame)
        {
            Return = ffmpeg.avcodec_receive_frame(Decoder.CodecContext, Decoder.Frame);
            if (Return == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                do
                {
                    do
                    {
                        ffmpeg.av_packet_unref(Decoder.Packet);
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

            frame = *Decoder.Frame;
            return true;

            EOF:
            Loop();
            frame = *Decoder.Frame;
            return false;
        }

        /// <summary>
        /// Dispose of all leaky varibles.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above. GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Attempts to get 1 frame of Video, or refill Audio buffer.
        /// </summary>
        /// <returns>Returns -1 if missing stream or returns AVERROR or returns 0 if no problem.</returns>
        public int Next()
        {
            if (Decoder.StreamIndex == -1 || State == FfccState.DONE)
            {
                return -1;
            }
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
            if (Decoder.StreamIndex > -1)
            {
                if (!timer.IsRunning && Mode == FfccMode.STATE_MACH && MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    timer.Start();
                }
                if (DynamicSound != null && !DynamicSound.IsDisposed && AudioEnabled)
                {
                    DynamicSound.Volume = volume;
                    DynamicSound.Pitch = pitch;
                    DynamicSound.Pan = pan;
                    DynamicSound.Play();
                }

                if (SoundEffect != null && !SoundEffect.IsDisposed && AudioEnabled)
                {
                    SoundEffectInstance.Volume = volume;
                    SoundEffectInstance.Pitch = pitch;
                    SoundEffectInstance.Pan = pan;
                    SoundEffectInstance.Play();
                }
            }
        }

        /// <summary>
        /// Stop playing Sound or Stop the FPS timer for Video , and Dispose of Varibles
        /// </summary>
        public void Stop()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                timer.Reset();
            }
            if (DynamicSound != null && !DynamicSound.IsDisposed)
            {
                if (AudioEnabled)
                {
                    DynamicSound.Stop();
                }

                DynamicSound.Dispose();
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

        /// <summary>
        /// Converts Frame to Texture with correct colorspace
        /// </summary>
        /// <returns>Texture2D</returns>
        public Texture2D Texture2D()
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

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (DecodedMemoryStream != null)
                {
                    DecodedMemoryStream.Dispose();
                }
                Stop();
                if (ConvertedData != null)
                {
                    Marshal.FreeHGlobal((IntPtr)ConvertedData);
                }
                if (_intPtr != null)
                {
                    Marshal.FreeHGlobal(_intPtr);
                }
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

                //if (avio_ctx_buffer != null)
                //    ffmpeg.av_freep(avio_ctx_buffer); //throws exception

                // set to true to prevent multiple disposings
                isDisposed = true;
                GC.Collect(); // donno if this really does much. was trying to make sure the memory i'm watching is what is really there.
            }
        }

        /// <summary>
        /// throw new exception
        /// </summary>
        /// <param name="v">string of message</param>
        private static void die(string v)
        {
            throw new Exception(v.Trim('\0'));
        }

        /// <summary>
        /// Compairs two numbers and returns the smallest
        /// </summary>
        /// <param name="a">number a</param>
        /// <param name="b">number b</param>
        /// <returns>smaller of two numbers</returns>
        private static int FFMIN(int a, int b)
        {
            return a < b ? a : b;
        }

        /// <summary>
        /// For reading data from a memory pointer as if it's a file.
        /// </summary>
        /// <param name="opaque">incoming data</param>
        /// <param name="buf">outgoing data</param>
        /// <param name="buf_size">outgoing buffer size</param>
        /// <returns>Total bytes read, or EOF</returns>
        private static int Read_packet(void* opaque, byte* buf, int buf_size)
        {
            Buffer_data* bd = (Buffer_data*)opaque;
            buf_size = FFMIN(buf_size, bd->size);

            if (buf_size <= 0)
            {
                return ffmpeg.AVERROR_EOF;
            }

            // copy internal buffer data to buf
            Buffer.MemoryCopy((void*)bd->ptr, (void*)buf, buf_size, buf_size);
            bd->ptr += buf_size;
            bd->size -= buf_size;

            return buf_size;
        }

        /// <summary>
        /// Converts FFMPEG error codes into a string.
        /// </summary>
        private string AvError(int ret)
        {
            ulong errbuff_size = 256;
            byte[] errbuff = new byte[errbuff_size];
            fixed (byte* ptr = &errbuff[0])
            {
                ffmpeg.av_strerror(ret, ptr, errbuff_size);
            }

            return Encoding.UTF8.GetString(errbuff).Trim('\0');
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
        /// reads the tags from metadata
        /// </summary>
        /// <param name="metadata">metadata from format or stream</param>
        private void GetTags(ref AVDictionary* metadata)
        {
            string val = "", key = "";
            AVDictionaryEntry* tag = null;
            while ((tag = ffmpeg.av_dict_get(metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
            {
                for (int i = 0; tag->value[i] != 0; i++)
                {
                    val += (char)tag->value[i];
                }

                for (int i = 0; tag->key[i] != 0; i++)
                {
                    key += (char)tag->key[i];
                }

                Metadata[key.ToUpper()] = val;
                if (key == "LOOPSTART" && int.TryParse(val, out _loopstart))
                { }
                key = "";
                val = "";
            }
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        private void Init(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH, int loopstart = -1)
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

        /// <summary>
        /// Sets up AVFormatContext to be able from the memory buffer.
        /// </summary>
        /// <param name="buffer">Pointer to data</param>
        /// <param name="buffer_size">Size of data</param>
        private void LoadFromRAM(IntPtr buffer, int buffer_size)
        {
            _avio_ctx = null;

            _avio_ctx_buffer_size = 4096;
            int ret = 0;
            _bufferData = new Buffer_data
            {
                ptr = buffer,
                size = buffer_size
            };

            _avio_ctx_buffer = (byte*)ffmpeg.av_malloc((ulong)_avio_ctx_buffer_size);
            if (_avio_ctx_buffer == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
                return;
            }
            avio_alloc_context_read_packet rf = new avio_alloc_context_read_packet(Read_packet);
            fixed (Buffer_data* tmp = &_bufferData)
            {
                _avio_ctx = ffmpeg.avio_alloc_context(_avio_ctx_buffer, _avio_ctx_buffer_size, 0, tmp, rf, null, null);
            }

            if (_avio_ctx == null)
            {
                ret = ffmpeg.AVERROR(ffmpeg.ENOMEM);
            }
            Decoder._format->pb = _avio_ctx;
            Open();
        }

        /// <summary>
        /// Copies byte[] data to a Pointer. So it can be used with ffmpeg.
        /// </summary>
        /// <param name="data">incoming data</param>
        /// <param name="length">size of data</param>
        private void LoadFromRAM(byte[] data, int length)
        {
            _intPtr = Marshal.AllocHGlobal(length);
            Marshal.Copy(data, 0, _intPtr, length);
            LoadFromRAM(_intPtr, length);
        }

        /// <summary>
        /// Load sound from Memorystream into a SoundEFFect
        /// </summary>
        /// <param name="decodedStream">Memory Stream containing sound data</param>
        private void LoadSoundFromStream(ref MemoryStream decodedStream)
        {
            if (DecodedMemoryStream.Length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (SoundEffect == null)
                {
                    SoundEffect = new SoundEffect(decodedStream.GetBuffer(), 0, (int)decodedStream.Length, ResampleFrame->sample_rate, (AudioChannels)ResampleFrame->channels, 0, 0);
                    SoundEffectInstance = SoundEffect.CreateInstance();
                    if (LOOPSTART >= 0)
                    {
                        SoundEffectInstance.IsLooped = true;
                    }
                }
            }
        }

        /// <summary>
        /// Add sound from byte[] to a DynamicSoundEFFectInstance, it can play while you give it more data.
        /// </summary>
        /// <param name="decodedStream">sound data</param>
        private void LoadSoundFromStream(ref byte[] buffer, int start, ref int length)
        {
            if (length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (DynamicSound == null)
                {
                    //create instance here to set sample_rate and channels dynamicly
                    DynamicSound = new DynamicSoundEffectInstance(ResampleFrame->sample_rate, (AudioChannels)ResampleFrame->channels);
                }

                DynamicSound.SubmitBuffer(buffer, 0, length);
            }
        }

        /// <summary>
        /// Load sound from memory stream by default.
        /// </summary>
        private void LoadSoundFromStream()
        {
            LoadSoundFromStream(ref _decodedMemoryStream);
        }

        /// <summary>
        /// If looping seek back to LOOPSTART
        /// </summary>
        private void Loop()
        {
            if (LOOPSTART >= 0 && Mode == FfccMode.STATE_MACH)
            {
                int min = LOOPSTART - 1000;
                if (min < 0)
                {
                    min = 0;
                }

                long max = Decoder.Stream->duration;
                if (max < 0)
                {
                    max = LOOPSTART + 1000;
                }
                //I didn't realize this didn't change the framenumber to 0. it just appends the LOOPSTART pos to the current stream.
                //So it is possible this could overflow when it's looped long enough to bring the currentframenum to max value.
                Return = ffmpeg.avformat_seek_file(Decoder.Format, Decoder.StreamIndex, min, LOOPSTART, max, 0);
                CheckReturn();

                State = FfccState.WAITING;
            }
        }

        /// <summary>
        /// Opens filename and assigns FormatContext.
        /// </summary>
        private int Open()
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
        private void PrepareCodec()
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
        private void PrepareResampler()
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

            ConvertedData = (byte*)Marshal.AllocHGlobal(convertedFrameBufferSize);
        }

        /// <summary>
        /// Setup scaler for drawing frames to bitmap.
        /// </summary>
        private void PrepareScaler()
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
        private void Process()
        {
            while (Decode(out AVFrame _DecodedFrame))
            {
                if (FrameSkip && Behind)
                {
                    continue;
                }

                if (MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    if (Mode == FfccMode.STATE_MACH)
                    {
                        State = FfccState.WAITING;
                        return;
                    }
                    // do something with video here.
                    else if (Mode == FfccMode.PROCESS_ALL)
                    {
                    }
                }
                else if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    Resample(ref _DecodedFrame);
                    if (Mode == FfccMode.STATE_MACH && !Behind)
                    {
                        State = FfccState.WAITING;
                        return;
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
        /// Resample current frame, Save output data
        /// </summary>
        /// <param name="frame">Current Decoded Frame</param>
        private void Resample(ref AVFrame frame)
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
                fixed (byte** tmp = &_convertedData)
                {
                    outSamples = ffmpeg.swr_convert(ResampleContext,
                                                tmp,
                                                ResampleFrame->nb_samples, null, 0);
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
                        if (State == FfccState.WAITING)
                        {
                            ret = 0;
                        }
                        else
                        {
                            ret = -1;
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

        /// <summary>
        /// Write to Memory Stream.
        /// </summary>
        /// <param name="output">Byte pointer to output buffer array</param>
        /// <param name="start">Start from typically 0</param>
        /// <param name="length">Total bytes to read.</param>
        /// <returns>bytes wrote</returns>
        private int WritetoMs(ref byte* output, int start, ref int length)
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

        #region Structs

        /// <summary>
        /// Used only when reading ADPCM data from memory.
        /// </summary>
        private struct Buffer_data
        {
            #region Fields

            public IntPtr ptr;
            public int size;

            #endregion Fields

            //< size left in the buffer
        };

        #endregion Structs
    }
}