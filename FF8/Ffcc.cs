using FFmpeg.AutoGen;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;



namespace FF8
{
    internal unsafe class Ffcc : IDisposable
    {
        //code converted to c# from c++
        // original code from https://rodic.fr/blog/libavcodec-tutorial-decode-audio-file/
        // mixed with https://libav.org/documentation/doxygen/master/decode__video_8c_source.html
        // some of aforge in there.

        private SwrContext* _resampleContext;
        private SwsContext* _scalerContext;
        private AVFrame* _resampleFrame;

        private byte* _convertedData = null;
        private MemoryStream _decodedStream;

        private FfccVaribleGroup Decoder { get; set; } = new FfccVaribleGroup();
        /// <summary>
        /// Most ffmpeg functions return an integer. If the value is less than 0 it is an error usually. Sometimes data is passed and then it will be greater than 0.
        /// </summary>
        private int Return { get; set; }
        private int GoalBufferCount { get => 30; }

        ///// <summary>
        ///// Packet of data can contain 1 or more frames.
        ///// </summary>
        //private AVPacket* Decoder.Packet { get => _Decoder.Packet; set => _Decoder.Packet = value; }
        ///// <summary>
        ///// Frame holds a chunk of data related to the current stream. 
        ///// </summary>
        //private AVFrame* Decoder.Frame { get => _Decoder.Frame; set => _Decoder.Frame = value; }
        /// <summary>
        /// Resample Context
        /// </summary>
        private SwrContext* ResampleContext { get => _resampleContext; set => _resampleContext = value; }
        /// <summary>
        /// SWS Context
        /// </summary>
        private SwsContext* ScalerContext { get => _scalerContext; set => _scalerContext = value; }
        /// <summary>
        /// Parser Context
        /// </summary>
        //private AVCodecParserContext* ParserContext { get => _parserContext; set => _parserContext = value; }

        /// <summary>
        /// Codec context, set from stream
        /// </summary>
        //private AVCodecContext* Decoder.CodecContext { get => _Decoder.CodecContext; set => _Decoder.CodecContext = value; }
        // public AVCodec* Decoder.Codec { get; private set; }

        /// <summary>
        /// pointer to stream
        /// </summary>
        private AVStream* EncoderStream { get; set; }
        /// <summary>
        /// pointer to stream
        /// </summary>
        private AVStream* DecoderStream { get; set; }
        /// <summary>
        /// index of stream
        /// </summary>
        private int DecoderStreamIndex { get; set; }
        ///// <summary>
        ///// number of channels being exported.
        ///// </summary>
        //private int Channels { get; set; }
        ///// <summary>
        ///// Audio sample_rate being exported
        ///// </summary>
        //private int Sample_rate { get; set; }
        /// <summary>
        /// True if file is open.
        /// </summary>
        public bool FileOpened { get; private set; }
        /// <summary>
        /// Path and filename of file.
        /// </summary>
        public string DecodedFileName { get; private set; }
        /// <summary>
        /// Current media type being processed.
        /// </summary>
        public AVMediaType MediaType { get; private set; }
        /// <summary>
        /// MemoryStream of Audio after decoding and resamping to compatable format.
        /// </summary>
        private MemoryStream DecodedStream { get => _decodedStream; set => _decodedStream = value; }
        public AVFrame* ResampleFrame { get => _resampleFrame; private set => _resampleFrame = value; }
        private FfccState State { get; set; }
        private FfccMode Mode { get; set; }
        private bool useduration => Decoder.CodecContext == null || (Decoder.CodecContext->framerate.den == 0 || Decoder.CodecContext->framerate.num == 0);
        private double videoFPS
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
        /// returns Frames per second or if that is 0. it will return the Time_Base ratio.
        /// This is the fundamental unit of time (in seconds) in terms of which frame timestamps are represented.
        ///  In many cases the audio files time base is  the same as the sample rate. example 1/44100.
        ///  video files  audio stream might be 1/100 or 1/1000. these can make for large durrations.
        /// </summary>
        public double FPS
        {
            get
            {
                double r;
                if (useduration)
                {
                    if ((r = videoFPS) > 0)
                    {
                        return r;
                    }
                    else if (DecoderStream != null && DecoderStream->time_base.den != 0)
                    {
                        return DecoderStream->time_base.num / (double)DecoderStream->time_base.den; // returns the time_base 
                    }
                }
                else if (Decoder.CodecContext != null && Decoder.CodecContext->framerate.den != 0)
                {
                    return Decoder.CodecContext->framerate.num / (double)Decoder.CodecContext->framerate.den;
                }

                return 0;
            }
        }

        /// <summary>
        /// if there is no stream it returns false. only checked when trying to process audio
        /// </summary>
        private bool AudioEnabled => DecoderStreamIndex >= 0;
        /// <summary>
        /// When getting video frames if behind it goes to next frame.
        /// disabled for debugging purposes.
        /// </summary>
        public bool FrameSkip { get; set; } = true;
        /// <summary>
        /// Stopwatch tracks the time audio has played so video can sync or loops can be looped.
        /// </summary>
        public Stopwatch timer { get; } = new Stopwatch();
        //public FileStream DecodeFileStream { get => _decodeFileStream; set => _decodeFileStream = value; }
        public enum FfccMode
        {
            /// <summary>
            /// Processes entire file at once and does something with output
            /// </summary>
            PROCESS_ALL,
            /// <summary>
            /// State machine, functions in this call update to update current state.
            /// And update decides what to do next.
            /// </summary>
            STATE_MACH,
            /// <summary>
            /// Not Init some error happened that prevented the class from working
            /// </summary>
            NOTINIT
        }
        public enum FfccState
        {
            INIT,
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
            READ
        }



        public Ffcc()
        {
            Mode = FfccMode.NOTINIT;
        }

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public Ffcc(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH)
        {
            Init(filename, mediatype, mode);
        }
        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public void Init(string filename, AVMediaType mediatype = AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode mode = FfccMode.STATE_MACH)
        {
            State = FfccState.INIT;
            Mode = mode;
            DecodedFileName = filename;
            MediaType = mediatype;
            Update();
        }
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
                    case FfccState.INIT:
                        State = FfccState.GETCODEC;
                        Init();
                        break;
                    case FfccState.GETCODEC:
                        State = FfccState.PREPARE_SWS;
                        FindOpenCodec();
                        break;
                    case FfccState.PREPARE_SWS:
                        State = FfccState.READ;
                        PrepareScaler();
                        break;
                    case FfccState.READ://Enters waiting state unless we want to process all now.
                        State = Mode == FfccMode.PROCESS_ALL ? FfccState.READALL : FfccState.WAITING;
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
        public void PlaySound(bool notimer = false) // there are some videos without sound meh.
        {

            if (!timer.IsRunning)
            {
                if (!notimer)
                {
                    timer.Start();
                }
                if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
                {
                    dsee44100.Play();
                }

                if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
                {
                    dsee48000.Play();
                }
            }
        }
        public void StopSound()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                timer.Reset();
            }
            if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
            {
                dsee44100.Stop();
            }

            if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
            {
                dsee48000.Stop();
            }
            if (dsee44100 != null && !dsee44100.IsDisposed)
            {
                dsee44100.Dispose();
            }

            if (dsee44100 != null && !dsee44100.IsDisposed)
            {
                dsee48000.Dispose();
            }
        }

        private void LoadSoundFromStream()
        {
            LoadSoundFromStream(ref _decodedStream);
        }

        private DynamicSoundEffectInstance dsee44100 = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
        private DynamicSoundEffectInstance dsee48000 = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);
        private int _loopstart = -1;

        public int LOOPSTART { get => _loopstart; private set => _loopstart = value; }
        public void LoadSoundFromStream(ref MemoryStream decodedStream)
        {
            if (DecodedStream.Length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                //    // accepts s16le maybe more haven't tested everything.

                if (ResampleFrame->sample_rate == 44100)
                {
                    dsee44100.SubmitBuffer(decodedStream.GetBuffer(), 0, (int)decodedStream.Length);
                }
                else if (ResampleFrame->sample_rate == 48000)
                {
                    dsee48000.SubmitBuffer(decodedStream.GetBuffer(), 0, (int)decodedStream.Length);
                }

            }
        }
        public void LoadSoundFromStream(ref byte[] buffer, int start, ref int length)
        {

            if (length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (ResampleFrame->sample_rate == 44100)
                {
                    dsee44100.SubmitBuffer(buffer, 0, length);
                }
                else if (ResampleFrame->sample_rate == 48000)
                {
                    dsee48000.SubmitBuffer(buffer, 0, length);
                }
            }
        }
        public int ExpectedFrame()
        {
            if (timer.IsRunning)
            {
                //if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                //{
                //    //int ret = (int)Math.Round(timer.ElapsedMilliseconds * ((double)75 / 1000));//FPS * 5
                //    //if (LOOPED && LOOPSTART >= 0)
                //    //    ret += (int)(((double)LOOPSTART / Decoder.CodecContext->sample_rate) *((double)75));
                //    return ret;
                //}
                //else
                return (int)Math.Round(timer.ElapsedMilliseconds * (FPS / 1000));
            }
            return 0;
        }
        public int CurrentFrameNum()
        {
            if (Decoder.CodecContext != null)
            {
                return Decoder.CodecContext->frame_number;
            }
            else
            {
                return -1;
            }
        }
        public bool AheadFrame()
        {
            if (timer.IsRunning && DecoderStreamIndex != -1)
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    return dsee44100.PendingBufferCount > GoalBufferCount;
                }

                return CurrentFrameNum() > ExpectedFrame();
            }
            return true;
        }
        //private long LOOPLENGTH
        //{
        //    get
        //    {
        //        if (LOOPSTART >= 0 && LOOPED)
        //        {

        //            return (DecoderStream->duration - LOOPSTART);
        //        }
        //        else
        //        {
        //            return DecoderStream->duration;
        //        }
        //    }
        //}
        private bool LOOPED = false;
        public bool BehindFrame()
        {
            if (timer.IsRunning && DecoderStreamIndex != -1)
            {
                //if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO && LOOPSTART >= 0 && FPS < 1 && DecoderStream->duration > 0)
                //{
                //    long time = timer.ElapsedMilliseconds;
                //    int secondsbeforeendoftrack = 2;
                //    return (time / 1000) > (LOOPLENGTH / Decoder.CodecContext->sample_rate) - secondsbeforeendoftrack;
                //}

                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    return dsee44100.PendingBufferCount < GoalBufferCount;
                }

                return CurrentFrameNum() < ExpectedFrame();

            }
            return false;
        }
        public bool CorrectFrame()
        {
            if (timer.IsRunning)
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    return dsee44100.PendingBufferCount == GoalBufferCount;
                }

                return CurrentFrameNum() == ExpectedFrame();
            }
            return false;
        }
        /// <summary>
        /// Adapted from example in FFmpeg.Autogen's example
        /// Decodes current stream;
        /// </summary>
        /// <param name="frame">Outputs current frame without</param>
        /// <returns>True if more data, False if EOF</returns>
        public bool Decode(out AVFrame frame)
        {
            ffmpeg.av_frame_unref(Decoder.Frame);
            do
            {
                try
                {
                    do
                    {
                        Return = ffmpeg.av_read_frame(Decoder.Format, Decoder.Packet);
                        if (Return == ffmpeg.AVERROR_EOF)
                        {
                            if (LOOPSTART >= 0)
                            {
                                ffmpeg.avformat_seek_file(Decoder.Format, DecoderStreamIndex, LOOPSTART - 1000, LOOPSTART, DecoderStream->duration, 0);


                                LOOPED = true; // set after one play to change durration checked.
                                State = FfccState.WAITING;
                                if (BehindFrame())
                                {
                                    timer.Restart();
                                }
                            }
                            frame = *Decoder.Frame;
                            return false;
                        }
                        else
                        {
                            CheckReturn();
                        }
                    } while (Decoder.Packet->stream_index != DecoderStreamIndex);


                    Return = ffmpeg.avcodec_send_packet(Decoder.CodecContext, Decoder.Packet);
                    //sent a eof when trying to loop once.
                    //should never get EOF here unless something is wrong.

                    CheckReturn();

                }
                finally
                {
                    ffmpeg.av_packet_unref(Decoder.Packet);
                }

                Return = ffmpeg.avcodec_receive_frame(Decoder.CodecContext, Decoder.Frame);
            } while (Return == ffmpeg.AVERROR(ffmpeg.EAGAIN));
            CheckReturn();

            frame = *Decoder.Frame;
            return true;
        }
        public static int DecodeFlush(ref AVCodecContext* avctx, ref AVPacket avpkt)
        {
            avpkt.data = null;
            avpkt.size = 0;

            fixed (AVPacket* tmpPacket = &avpkt)
            {
                return ffmpeg.avcodec_send_packet(avctx, tmpPacket);
            }
        }
        public static int EncodeFlush(AVCodecContext* avctx)
        {
            if (avctx != null)
            {
                return ffmpeg.avcodec_send_frame(avctx, null);
            }

            return 0;
        }

        /// <summary>
        /// Decodes, Resamples, Encodes
        /// </summary>
        /// <ref>https://stackoverflow.com/questions/32051847/c-ffmpeg-distorted-sound-when-converting-audio?rq=1#_=_</ref>
        /// <param name="skipencode">skip encoding</param>
        private void Process(bool skipencode = true)
        {

            if (CurrentFrameNum() == 0)
            {
                PrepareResampler(skipencode);
            }
            //int frameFinished = 0;
            while (Decode(out AVFrame _DecodedFrame))
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    if (Mode == FfccMode.STATE_MACH)
                    {
                        State = FfccState.WAITING;
                        return;
                    }
                    // do something with video here.
                    else if (!skipencode)
                    {
                        BMP_Save();
                    }
                }
                else if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {

                    Resample(ref _DecodedFrame, skipencode);
                    if (Mode == FfccMode.STATE_MACH)
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
        private void Resample(ref AVFrame frame, bool skipencode = true)
        {
            // do something with audio here.
            if (!NoResample(ref frame))
            {
                // Convert
                int outSamples = 0;
                fixed (byte** tmp = (byte*[])frame.data)
                {
                    outSamples = ffmpeg.swr_convert(ResampleContext, null, 0,
                                 //&convertedData,
                                 //SwrFrame->nb_samples,
                                 tmp,
                         frame.nb_samples);
                }
                if (outSamples < 0)
                {
                    die("Could not convert");
                }
                for (; ; )
                {
                    outSamples = ffmpeg.swr_get_out_samples(ResampleContext, 0);
                    if ((outSamples < (Decoder.CodecContext == null ? 0 : Decoder.CodecContext->frame_size) * ResampleFrame->channels) || (Decoder.CodecContext == null ? 0 : Decoder.CodecContext->frame_size) == 0 && (outSamples < ResampleFrame->nb_samples * ResampleFrame->channels))
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
        }

        private void PrepareProcess()
        {
            bool skipencoder = true;

            using (DecodedStream = new MemoryStream())
            {
                Process(skipencoder);
                LoadSoundFromStream();
            }
        }
        ~Ffcc()
        {
            Dispose();
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
                }

                CheckReturn();
                Return = ffmpeg.avformat_find_stream_info(Decoder.Format, null);

                CheckReturn();
                GetTags(ref Decoder.Format->metadata);

                FileOpened = true;
            }
            return (FileOpened) ? 0 : -1;
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
        /// Init Class
        /// </summary>
        private void Init()
        {
            //default values below here.
            Return = -1;
                if (Open() < 0)
                {
                    die("No file not open");
                }

                ConvertedData = null;


                DecoderStream = null;

                DecoderStreamIndex = -1;



        }

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
        public Dictionary<String, String> Metadata { get; private set; } = new Dictionary<string, string>();
        public byte* ConvertedData { get => _convertedData; private set => _convertedData = value; }
        /// <summary>
        /// Finds the codec for the chosen stream
        /// </summary>
        private void FindOpenCodec()
        {
            // find & open codec
            fixed (AVCodec** tmp = &Decoder._codec)
            {
                Return = ffmpeg.av_find_best_stream(Decoder.Format, MediaType, -1, -1, tmp, 0);
            }
            if (Return == ffmpeg.AVERROR_STREAM_NOT_FOUND && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                State = FfccState.DONE;
                Mode = FfccMode.NOTINIT;
                return;
            }
            else
            {
                CheckReturn();
            }

            DecoderStreamIndex = Return;
            DecoderStream = Decoder.Format->streams[Return];
            GetTags(ref DecoderStream->metadata);
            Decoder.CodecContext = ffmpeg.avcodec_alloc_context3(Decoder.Codec);
            if (Decoder.CodecContext == null)
            {
                die("Could not allocate codec context");
            }

            Return = ffmpeg.avcodec_parameters_to_context(Decoder.CodecContext, DecoderStream->codecpar);
            CheckReturn();
            Return = ffmpeg.avcodec_open2(Decoder.CodecContext, Decoder.Codec, null);
            CheckReturn();
            if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (Decoder.CodecContext->channel_layout == 0)
                {
                    Decoder.CodecContext->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
                }
            }
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
        /// PrepareResampler
        /// </summary>
        private void PrepareResampler(bool skipencode = true)
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

            ffmpeg.av_opt_set_channel_layout(ResampleContext, "out_channel_layout", ffmpeg.AV_CH_LAYOUT_STEREO, 0);
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

            ResampleFrame->nb_samples = 32;
            ResampleFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
            ResampleFrame->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
            ResampleFrame->channels = (int)AudioChannels.Stereo;
            ResampleFrame->sample_rate = Decoder.CodecContext->sample_rate;



            int convertedFrameBufferSize = ffmpeg.av_samples_get_buffer_size(null, ResampleFrame->channels,
                                                 ResampleFrame->nb_samples,
                                                 (AVSampleFormat)ResampleFrame->format, 0);

            ConvertedData = (byte*)Marshal.AllocHGlobal(convertedFrameBufferSize);
        }


        /// <summary>
        /// throw new exception
        /// </summary>
        /// <param name="v">string of message</param>
        private static void die(string v)
        {
            throw new Exception(v);
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

            return Encoding.UTF8.GetString(errbuff);
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
            else //memory leaky? seems when i used this method alot of memory wouldn't get disposed
            {
                long c_len = DecodedStream.Length;
                for (int i = start; i < length; i++)
                {
                    DecodedStream.WriteByte(output[i]);
                }
                if (DecodedStream.Length - c_len != length)
                {
                    die("not all data wrote");
                }

                return (int)(DecodedStream.Length - c_len);
            }
        }
        /// <summary>
        /// Tests audio against format type to see if can play it
        /// without resampling.
        /// </summary>
        /// <returns>True if decoded sample is good
        /// False if incompatable
        /// </returns>
        private bool NoResample(ref AVFrame frame)
        {
            // Adapted from https://libav.org/documentation/doxygen/master/decode_audio_8c-example.html
            /* the stream parameters may change at any time, check that they are
            * what we expect */
            /* The decoded data is signed 16-bit planar -- each channel in its own
             * buffer. We interleave the two channels manually here, but using
             * libavresample is recommended instead. */
            if (Decoder.CodecContext->channels == 2 && frame.format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16P)//not tested
            {
                if (Mode == FfccMode.STATE_MACH)
                {
                    int len = frame.linesize[0] * 2;
                    byte[] arr = new byte[len];

                    int j = 0;
                    for (int i = 0; i < frame.linesize[0]; i++)
                    {
                        arr[j++] = Marshal.ReadByte((IntPtr)(frame.data[0]), i);
                        arr[j++] = Marshal.ReadByte((IntPtr)(frame.data[1]), i);
                    }

                    LoadSoundFromStream(ref arr, 0, ref len);
                }
                else
                {
                    for (int i = 0; i < frame.linesize[0]; i++)
                    {
                        DecodedStream.WriteByte(((byte*[])frame.data)[0][i]);
                        DecodedStream.WriteByte(((byte*[])frame.data)[1][i]);
                    }
                }
                return true;
            }
            else if (frame.format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16) // played eyes on me wav
            {

                if (Mode == FfccMode.STATE_MACH)
                {
                    int len = frame.linesize[0];
                    byte[] arr = new byte[len];

                    for (int i = 0; i < frame.linesize[0]; i++)
                    {
                        arr[i] = Marshal.ReadByte((IntPtr)(frame.data[0]), i);
                    }
                    LoadSoundFromStream(ref arr, 0, ref len);
                }
                else
                {
                    for (int i = 0; i < frame.linesize[0]; i++)
                    {
                        DecodedStream.WriteByte(((byte*[])frame.data)[0][i]);
                    }
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// Attempts to get atleast one frame. Video gets 1. Audio gets the whole file.
        /// </summary>
        /// <returns>Returns -1 if missing stream or returns AVERROR or returns 0 if no problem.</returns>
        public int GetFrame()
        {
            if (DecoderStreamIndex == -1)
            {
                return -1;
            }
            else
            {
                return Update(FfccState.READONE);
            }
        }
        /// <summary>
        /// Saves each frame to it's own BMP image file.
        /// </summary>
        private void BMP_Save()
        {
            string filename = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(DecodedFileName)}_rawframe.{Decoder.CodecContext->frame_number}.bmp");
            using (FileStream fs = File.OpenWrite(filename))
            {
                using (Bitmap bitmap = FrameToBMP())
                {
                    bitmap.Save(fs, ImageFormat.Bmp);
                }
            }
        }
        /// <summary>
        /// Converts raw video frame to correct colorspace
        /// </summary>
        /// <remarks>AForge source for refernce this function. was c++.</remarks>
        /// <returns>Bitmap of frame</returns>
        public Bitmap FrameToBMP()
        {
            Bitmap bitmap = null;
            BitmapData bitmapData = null;

            try
            {
                bitmap = new Bitmap(Decoder.CodecContext->width, Decoder.CodecContext->height, PixelFormat.Format32bppArgb);
                AVPixelFormat v = Decoder.CodecContext->pix_fmt;

                // lock the bitmap
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, Decoder.CodecContext->width, Decoder.CodecContext->height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte* ptr = (byte*)(bitmapData.Scan0);

                byte*[] srcData = { ptr, null, null, null };
                int[] srcLinesize = { bitmapData.Stride, 0, 0, 0 };

                // convert video frame to the RGB bitmap
                ffmpeg.sws_scale(ScalerContext, Decoder.Frame->data, Decoder.Frame->linesize, 0, Decoder.CodecContext->height, srcData, srcLinesize);


                return bitmap;
            }
            finally
            {
                if (bitmap != null && bitmapData != null)
                {
                    bitmap.UnlockBits(bitmapData);
                }
            }
        }
        /// <summary>
        /// Converts BMP to Texture
        /// </summary>
        /// <returns>Texture2D</returns>
        public Texture2D FrameToTexture2D()
        {
            //Get Bitmap. there might be a way to skip this step.
            using (Bitmap frame = FrameToBMP())
            {
                //Create Texture
                Texture2D frameTex = new Texture2D(Memory.spriteBatch.GraphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color); //GC will collect frameTex
                                                                                                                                              //Fill it with the bitmap.
                BitmapData bmpdata = frame.LockBits(new Rectangle(0, 0, frame.Width, frame.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);// System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                byte[] texBuffer = new byte[bmpdata.Width * bmpdata.Height * 4]; //GC here
                Marshal.Copy(bmpdata.Scan0, texBuffer, 0, texBuffer.Length);
                frame.UnlockBits(bmpdata);
                frameTex.SetData(texBuffer);


                return frameTex;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                if (DecodedStream != null)
                {
                    DecodedStream.Dispose();
                }
                StopSound();
                if (ConvertedData != null && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    Marshal.FreeHGlobal((IntPtr)ConvertedData);
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
                disposedValue = true;
                GC.Collect(); // donno if this really does much. was trying to make sure the memory i'm watching is what is really there.
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Ffcc() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
