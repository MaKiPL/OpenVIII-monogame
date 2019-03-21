using FFmpeg.AutoGen;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using NAudio.Wave;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;



namespace FF8
{
    public sealed class ManualMemoryStream : MemoryStream
    {
        //https://stackoverflow.com/questions/2620851/avoiding-dispose-of-underlying-stream
        protected override void Dispose(bool disposing)
        {
        }
        public void ManualDispose()
        {
            base.Dispose(true);
        }
    }
    unsafe class Ffcc
    {
        //code converted to c# from c++
        // original code from https://rodic.fr/blog/libavcodec-tutorial-decode-audio-file/
        // mixed with https://libav.org/documentation/doxygen/master/decode__video_8c_source.html
        // some of aforge in there.
        /// <summary>
        /// Method of Format, required for atleast one function.
        /// </summary>
        private AVFormatContext* _format; // FormatContext
        private AVFrame* _frame;
        private AVPacket* _packet;
        private SwrContext* _swr;
        private SwsContext* _sws;
        private AVFrame* _swrFrame;
        private static readonly WaveOut wo;

        /// <summary>
        /// Most ffmpeg functions return an integer. If the value is less than 0 it is an error usually. Sometimes data is passed and then it will be greater than 0.
        /// </summary>
        private int Ret { get; set; }
        /// <summary>
        /// Format holds alot of file info. File is opened and data about it is stored here.
        /// </summary>
        private AVFormatContext* Format { get => _format; set => _format = value; }
        /// <summary>
        /// Packet of data can contain 1 or more frames.
        /// </summary>
        private AVPacket* Packet { get => _packet; set => _packet = value; }
        /// <summary>
        /// Frame holds a chunk of data related to the current stream. 
        /// </summary>
        private AVFrame* Frame { get => _frame; set => _frame = value; }
        /// <summary>
        /// Resample Context
        /// </summary>
        private SwrContext* Swr { get => _swr; set => _swr = value; }
        /// <summary>
        /// SWS Context
        /// </summary>
        private SwsContext* Sws { get => _sws; set => _sws = value; }
        /// <summary>
        /// Parser Context
        /// </summary>
        private AVCodecParserContext* Parser { get; set; }

        /// <summary>
        /// Codec context, set from stream
        /// </summary>
        private AVCodecContext* Codec { get; set; }
        /// <summary>
        /// pointer to stream
        /// </summary>
        private AVStream* out_audioStream { get; set; }
        /// <summary>
        /// pointer to stream
        /// </summary>
        private AVStream* Stream { get; set; }
        /// <summary>
        /// index of stream
        /// </summary>
        private int Stream_index { get; set; }
        /// <summary>
        /// number of channels being exported.
        /// </summary>
        private int Channels { get; set; }
        /// <summary>
        /// Audio sample_rate being exported
        /// </summary>
        private int Sample_rate { get; set; }
        /// <summary>
        /// True if file is open.
        /// </summary>
        public bool File_Opened { get; private set; }
        /// <summary>
        /// Path and filename of file.
        /// </summary>
        public string File_Name { get; private set; }
        /// <summary>
        /// Current media type being processed.
        /// </summary>
        public AVMediaType Media_Type { get; private set; }
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
            PACKET,
            /// <summary>
            /// Read a packet of data from file
            /// </summary>
            READONE,
            /// <summary>
            /// Get frames from packet4
            /// </summary>
            FRAME,
            /// <summary>
            /// Missing DLLs caused exception
            /// </summary>
            NODLL
        }

        private ManualMemoryStream Ms { get; set; }
        public AVFrame* SwrFrame { get => _swrFrame; private set => _swrFrame = value; }
        FfccState State { get; set; }
        FfccMode Mode { get; set; }
        public int FPS { get => (Codec != null ? (Codec->framerate.den == 0 || Codec->framerate.num == 0 ? 0 : Codec->framerate.num / Codec->framerate.den) : 0); }
        public int FrameRenderingDelay { get => (1000 / FPS) / 2; }
        public static SoundEffectInstance see { get; private set; }
        private static SoundEffect se { get; set; }
        public AVCodecContext* OutCodec { get; private set; }
        public AVFormatContext* OutFormat { get; private set; }
        public string Outfile { get; private set; }
        public string OutfileType { get; private set; }

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
            File_Name = filename;
            Media_Type = mediatype;
            Update();
            //Parse(); //doesn't seem to work. Or it only works with raw streams.
            //Decode(); //Parse() or Read() runs decode. Decode just tries to read the data in the packets. Default there is nothing there.
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
                        Init();
                        if (Mode == FfccMode.PROCESS_ALL)
                        {
                            State = FfccState.READALL;
                        }
                        else
                        {
                            State = FfccState.WAITING;
                        }

                        break;
                    case FfccState.READALL:
                        ReadAll();
                        State = FfccState.DONE;
                        break;
                    case FfccState.READONE:
                        return ReadOne();
                    case FfccState.PACKET:
                        return GetPacket();
                    case FfccState.FRAME:
                        return GetFrame();
                    default:
                        State = FfccState.DONE;
                        break;
                }
            }
            while (!((Mode == FfccMode.PROCESS_ALL && State == FfccState.DONE) || (State == FfccState.DONE || State == FfccState.WAITING)));
            return ret;
        }
        static FileStream wr;
        public static void StopSound()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                timer.Reset();
                see.Stop();
            }
        }
        public static bool FrameSkip { get; set; } = true; // when getting video frames if behind it goes to next frame.
        public static void PlaySound() // there are some videos without sound meh.
        {
            if (!timer.IsRunning)
            {
                timer.Start();
                see.Play();
            }
        }
        public int ExpectedFrame()
        {
            if (timer.IsRunning)
            {
                TimeSpan ts = timer.Elapsed;
                return (int)Math.Round(ts.TotalMilliseconds * ((double)FPS / 1000));
            }
            return 0;
        }
        public int CurrentFrameNum()
        {
            return Codec->frame_number;
        }
        public bool Ahead()
        {
            if (timer.IsRunning)
            {
                return CurrentFrameNum() > ExpectedFrame();
            }
            return false;
        }
        public bool Behind()
        {
            if (timer.IsRunning)
            {
                return CurrentFrameNum() < ExpectedFrame();
            }
            return false;
        }
        public bool Correct()
        {
            if (timer.IsRunning)
            {
                return CurrentFrameNum() > ExpectedFrame();
            }
            return false;
        }
        public static Stopwatch timer { get; } = new Stopwatch();
        private bool SoundExistsAndReady()
        {
            // I'm not sure how to write this better.
            //Outfile = Path.Combine(Path.GetDirectoryName(Outfile), $"{Path.GetFileNameWithoutExtension(Outfile)}.pcm");
            //wr = new WaveFileReader(@"C:\eyes_on_me.wav");//Outfile))
            if (File.Exists(Outfile))
            {
                wr = File.OpenRead(Outfile);
                if (wr.Length > 0)
                {
                    Ms = new ManualMemoryStream();
                    wr.CopyTo(Ms);
                    wr.Dispose();

                    se = new SoundEffect(Ms.ToArray(), Codec->sample_rate, (AudioChannels)Codec->channels);
                    see = se.CreateInstance();
                    //see.Play();
                    Ms.ManualDispose();
                    return true;
                }
            }

            return false;
        }
        public static int DecodeNext(AVCodecContext* avctx, AVFrame* frame, ref int got_frame_ptr, AVPacket* avpkt)
        {
            int ret = 0;
            got_frame_ptr = 0;
            if ((ret = ffmpeg.avcodec_receive_frame(avctx, frame)) == 0)
            {
                //0 on success, otherwise negative error code
                got_frame_ptr = 1;
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                //AVERROR(EAGAIN): input is not accepted in the current state - user must read output with avcodec_receive_packet()
                //(once all output is read, the packet should be resent, and the call will not fail with EAGAIN)
                ret = Decode(avctx, frame, ref got_frame_ptr, avpkt);
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                die("AVERROR_EOF: the encoder has been flushed, and no new frames can be sent to it");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                die("AVERROR(EINVAL): codec not opened, refcounted_frames not set, it is a decoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                die("Failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                die("unknown");
            }
            return ret;
        }
        public static int Decode(AVCodecContext* avctx, AVFrame* frame, ref int got_frame_ptr, AVPacket* avpkt)
        {
            int ret = 0;
            got_frame_ptr = 0;
            if ((ret = ffmpeg.avcodec_send_packet(avctx, avpkt)) == 0)
            {
                //0 on success, otherwise negative error code
                return DecodeNext(avctx, frame, ref got_frame_ptr, avpkt);
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                die("input is not accepted in the current state - user must read output with avcodec_receive_frame()(once all output is read, the packet should be resent, and the call will not fail with EAGAIN");
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                die("AVERROR_EOF: the decoder has been flushed, and no new packets can be sent to it (also returned if more than 1 flush packet is sent");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                die("codec not opened, it is an encoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                die("Failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                die("unknown");
            }
            return ret;//ffmpeg.avcodec_decode_audio4(fileCodecContext, audioFrameDecoded, &frameFinished, &inPacket);
        }
        public static int DecodeFlush(AVCodecContext* avctx, AVPacket* avpkt)
        {
            avpkt->data = null;
            avpkt->size = 0;
            return ffmpeg.avcodec_send_packet(avctx, avpkt);
        }
        public static int EncodeNext(AVCodecContext* avctx, AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        {
            int ret = 0;
            got_packet_ptr = 0;
            if ((ret = ffmpeg.avcodec_receive_packet(avctx, avpkt)) == 0)
            {
                got_packet_ptr = 1;
                //0 on success, otherwise negative error code
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                //output is not available in the current state - user must try to send input
                return Encode(avctx, avpkt, frame, ref got_packet_ptr);
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                die("AVERROR_EOF: the encoder has been fully flushed, and there will be no more output packets");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                die("AVERROR(EINVAL) codec not opened, or it is an encoder other errors: legitimate decoding errors");
            }
            else
            {
                die("unknown");
            }
            return ret;//ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished)
        }
        public static int Encode(AVCodecContext* avctx, AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        {
            int ret = 0;
            got_packet_ptr = 0;
            if ((ret = ffmpeg.avcodec_send_frame(avctx, frame)) == 0)
            {
                //0 on success, otherwise negative error code
                return EncodeNext(avctx, avpkt, frame, ref got_packet_ptr);
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                die("input is not accepted in the current state - user must read output with avcodec_receive_packet() (once all output is read, the packet should be resent, and the call will not fail with EAGAIN)");
            }
            else if (ret == ffmpeg.AVERROR_EOF)
            {
                die("AVERROR_EOF: the decoder has been flushed, and no new packets can be sent to it (also returned if more than 1 flush packet is sent");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
            {
                die("AVERROR(ffmpeg.EINVAL) codec not opened, refcounted_frames not set, it is a decoder, or requires flush");
            }
            else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
            {
                die("AVERROR(ENOMEM) failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
            }
            else
            {
                die("unknown");
            }
            return ret;//ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished)
        }
        public static int EncodeFlush(AVCodecContext* avctx)
        {
            return ffmpeg.avcodec_send_frame(avctx, null);
        }
        private void ReadAllnew()
        {
            int frameFinished = 0;
            PrepareResampler();
            for (; ; )
            {
                //if (Packet.size == 0|| Packet.stream_index != streamId)
                if (ffmpeg.av_read_frame(Format, Packet) < 0)
                {
                    break;
                }

                if (Packet->stream_index == Stream_index)
                {
                    int len = Decode(Codec, Frame, ref frameFinished, Packet);
                    if (len == ffmpeg.AVERROR_EOF)
                    {
                        break;
                    }
                    //int len = ffmpeg.avcodec_decode_audio4(Codec, Frame, &frameFinished, &Packet);

                    if (frameFinished != 0)
                    {

                        // Convert

                        byte* convertedData = null;

                        if (ffmpeg.av_samples_alloc(&convertedData,
                                     null,
                                     OutCodec->channels,
                                     SwrFrame->nb_samples,
                                     OutCodec->sample_fmt, 0) < 0)
                        {
                            die("Could not allocate samples");
                        }
                        int outSamples = 0;
                        fixed (byte** tmp = (byte*[])Frame->data)
                        {
                            outSamples = ffmpeg.swr_convert(Swr, null, 0,
                                         //&convertedData,
                                         //SwrFrame->nb_samples,
                                         tmp,
                                 Frame->nb_samples);
                        }
                        if (outSamples < 0)
                        {
                            die("Could not convert");
                        }

                        for (; ; )
                        {
                            outSamples = ffmpeg.swr_get_out_samples(Swr, 0);
                            if ((outSamples < OutCodec->frame_size * OutCodec->channels) || OutCodec->frame_size == 0 && (outSamples < SwrFrame->nb_samples * OutCodec->channels))
                            {
                                break; // see comments, thanks to @dajuric for fixing this
                            }

                            outSamples = ffmpeg.swr_convert(Swr,
                                                     &convertedData,
                                                     SwrFrame->nb_samples, null, 0);

                            int buffer_size = ffmpeg.av_samples_get_buffer_size(null,
                                           OutCodec->channels,
                                           SwrFrame->nb_samples,
                                           OutCodec->sample_fmt,
                                           0);
                            if (buffer_size < 0)
                            {
                                die("Invalid buffer size");
                            }
                            WritetoMs(convertedData, 0, buffer_size);
                            continue;
                            //encode
                            if (ffmpeg.avcodec_fill_audio_frame(SwrFrame,
                                     OutCodec->channels,
                                     OutCodec->sample_fmt,
                                     convertedData,
                                     buffer_size,
                                     0) < 0)
                            {
                                die("Could not fill frame");
                            }
                            AVPacket outPacket;
                            ffmpeg.av_init_packet(&outPacket);
                            outPacket.data = null;
                            outPacket.size = 0;
                            if (Encode(OutCodec, &outPacket, SwrFrame, ref frameFinished) < 0)
                            {
                                die("Error encoding audio frame");
                            }

                            //if (ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished) < 0)
                            //    die("Error encoding audio frame");
                            //if (audioStream->codec->coded_frame != null && audioStream->codec->coded_frame->pts != (int)ffmpeg.AV_NOPTS_VALUE)
                            //    outPacket.pts = ffmpeg.av_rescale_q(audioStream->codec->coded_frame->pts,
                            //        audioStream->codec->time_base, audioStream->codec->time_base);


                            //outPacket.flags |= ffmpeg.AV_PKT_FLAG_KEY;
                            outPacket.stream_index = out_audioStream->index;
                            //outPacket.data = audio_outbuf;
                            outPacket.dts = Frame->pkt_dts;
                            outPacket.pts = Frame->pkt_pts;
                            ffmpeg.av_packet_rescale_ts(&outPacket, Stream->time_base, out_audioStream->time_base);

                            if (frameFinished != 0)
                            {


                                if (ffmpeg.av_interleaved_write_frame(OutFormat, &outPacket) != 0)
                                {
                                    die("Error while writing audio frame");
                                }

                                ffmpeg.av_packet_unref(&outPacket);
                            }
                        }
                    }
                }
            }
            EncodeFlush(OutCodec);
            DecodeFlush(Codec, Packet);
            ffmpeg.avio_close(OutFormat->pb);
        }
        private void ReadAll()
        {
            //if (SoundExistsAndReady())
            //{
            //    return;
            //}

            Ms = new ManualMemoryStream();
            ReadAllnew();
            //while (true)
            //{
            //    Ret = ffmpeg.av_read_frame(Format, Packet);
            //    if (Ret == ffmpeg.AVERROR_EOF)
            //        break;
            //    CheckRet();
            //    Decode();
            //}
            if (Ms.Length > 0)
            {
                //try
                //{
                //    // accepts streams with s16le wave files maybe more haven't tested everything.
                //    se = SoundEffect.FromStream(Ms);
                //    see = se.CreateInstance();
                //    see.Play();
                //}
                //catch (ArgumentException)
                //{
                // accepts s16le maybe more haven't tested everything.
                se = new SoundEffect(Ms.ToArray(), SwrFrame->sample_rate, (AudioChannels)Channels);
                see = se.CreateInstance();
                see.Play();
                //}
            }
            //SoundExistsAndReady();
            Ms.ManualDispose();
        }
        ~Ffcc()
        {
            try
            {
                fixed (AVFrame** tmp = &_frame)
                {
                    ffmpeg.av_frame_free(tmp);
                }

                fixed (AVFrame** tmp = &_swrFrame)
                {
                    ffmpeg.av_frame_free(tmp);
                }

                fixed (AVPacket** tmp = &_packet)
                {
                    ffmpeg.av_packet_free(tmp);
                }

                fixed (SwrContext** tmp = &_swr)
                {
                    ffmpeg.swr_free(tmp);
                }

                ffmpeg.av_parser_close(Parser);
                ffmpeg.avcodec_close(Codec);
                ffmpeg.avcodec_close(OutCodec);
                ffmpeg.avformat_free_context(OutFormat);
                ffmpeg.avformat_free_context(Format);
            }
            catch (DllNotFoundException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.WriteLine("Clean up failed, maybe FFmpeg DLLs are missing");
            }
            try
            {
                //Ms.Dispose();
                Ms.ManualDispose();
            }
            catch (NullReferenceException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.WriteLine("memory stream already disposed of");
            }
            //try
            //{
            //    if (!see.IsDisposed)
            //        see.Dispose();
            //}
            //catch (NullReferenceException e)
            //{
            //    TextWriter errorWriter = Console.Error;
            //    errorWriter.WriteLine(e.Message);
            //    errorWriter.WriteLine("sound effect instance is already disposed of");
            //}

        }
        /// <summary>
        /// Opens filename and assigns FormatContext.
        /// </summary>
        private int Open()
        {
            if (!File_Opened)
            {
                fixed (AVFormatContext** tmp = &_format)
                {
                    Ret = ffmpeg.avformat_open_input(tmp, File_Name, null, null);
                }

                CheckRet();
                Ret = ffmpeg.avformat_find_stream_info(_format, null);
                CheckRet();
                File_Opened = true;

            }
            return (File_Opened) ? 0 : -1;
        }
        /// <summary>
        /// Throws exception if Ret is less than 0
        /// </summary>
        private int CheckRet()
        {
            switch (Ret)
            {
                case ffmpeg.AVERROR_OUTPUT_CHANGED:
                    die($"The swr_context output ch_layout, sample_rate, sample_fmt must match outframe! {Ret} - {AvError(Ret)}");
                    break;
                case ffmpeg.AVERROR_INPUT_CHANGED:
                    die($"The swr_context input ch_layout, sample_rate, sample_fmt must match inframe! {Ret} - {AvError(Ret)}");
                    break;
                default:
                    if (Ret < 0)
                    {
                        die($"{Ret} - {AvError(Ret)}");
                    }

                    break;
            }
            return Ret;
        }
        /// <summary>
        /// Init Class
        /// </summary>
        private void Init()
        {
            //default values below here.
            Ret = -1;
            Outfile = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(File_Name)}.pcm");
            OutfileType = "s16le";
            try
            {
                _format = ffmpeg.avformat_alloc_context();
                //Swr = ffmpeg.swr_alloc();
                Parser = null;
                Codec = null;
                Stream = null;
                Packet = ffmpeg.av_packet_alloc();
                if (Packet == null)
                {
                    die("Error allocating the packet\n");
                }
                //ffmpeg.av_init_packet(Packet);
                SwrFrame = ffmpeg.av_frame_alloc();
                if (SwrFrame == null)
                {
                    die("Error allocating the frame\n");
                }

                Frame = ffmpeg.av_frame_alloc();
                if (Frame == null)
                {
                    die("Error allocating the frame\n");
                }

                Stream_index = -1;
                Channels = 0;
                Sample_rate = 44100;

                //ffmpeg.av_register_all(); //should not need this.


                if (Open() < 0)
                {
                    die("No file not open");
                }

                Get_Stream();
                FindOpenCodec();
                Get_Stream();
                if (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    PrepareSWS();
                }
                //Moved this to decoder and changed it to prepair the outframe for resampling

                //if (Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                //    PrepareResampler();
            }
            catch (DllNotFoundException e)
            {
                State = FfccState.NODLL;
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.WriteLine("FFCC can't init due to missing ffmpeg dlls");
            }
        }
        /// <summary>
        /// Gets the first stream of definded type
        /// </summary>
        private void Get_Stream()
        {
            Stream_index = -1;
            // Find the index of the first audio stream
            for (int i = 0; i < Format->nb_streams; i++)
            {
                if (Format->streams[i]->codec->codec_type == Media_Type)
                {
                    Stream_index = i;
                    break; // only grab the first stream
                }
            }
            if (Stream_index == -1)
            {
                die($"Could not retrieve {(Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO ? "audio" : (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO ? "video" : "other"))} stream from file \n {File_Name}");
            }

            Stream = Format->streams[Stream_index];
        }
        /// <summary>
        /// Finds the codec for the chosen stream
        /// </summary>
        private void FindOpenCodec()
        {
            // find & open codec
            Codec = Stream->codec;
            AVCodec* c = ffmpeg.avcodec_find_decoder(Codec->codec_id);
            if (c == null)
            {
                die("Codec not found");
            }
            //Parser = ffmpeg.av_parser_init((int)c->id);
            //if (Parser == null)
            //    die("parser not found");
            //Commented due to Stream->codec doing the work for it.
            //Codec = ffmpeg.avcodec_alloc_context3(c);
            //if (Codec == null)
            //    die("Could not allocate video codec context");
            Ret = ffmpeg.avcodec_open2(Codec, c, null);
            CheckRet();
            if (Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (Codec->channel_layout == 0)
                {
                    Codec->channel_layout = ffmpeg.AV_CH_FRONT_LEFT | ffmpeg.AV_CH_FRONT_RIGHT;
                }
                Channels = Codec->channels;
            }
        }
        private void PrepareSWS()
        {
            Sws = ffmpeg.sws_getContext(
                Codec->width, Codec->height, Codec->pix_fmt,
                Codec->width, Codec->height, AVPixelFormat.AV_PIX_FMT_RGBA,
                ffmpeg.SWS_ACCURATE_RND, null, null, null);
            Ret = ffmpeg.sws_init_context(Sws, null, null);

            CheckRet();
        }
        private unsafe AVStream* add_audio_stream(AVFormatContext* oc, AVCodecID codec_id)
        {
            AVCodecContext* c;
            AVCodec* encoder = ffmpeg.avcodec_find_encoder(codec_id);
            AVStream* st = ffmpeg.avformat_new_stream(oc, encoder);

            if (st == null)
            {
                die("av_new_stream");
            }

            c = st->codec;
            c->codec_id = codec_id;
            c->codec_type = AVMediaType.AVMEDIA_TYPE_AUDIO;

            /* put sample parameters */
            c->bit_rate = 64000;
            c->sample_rate = Stream->codec->sample_rate;
            c->channels = Stream->codec->channels;
            c->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;//encoder->sample_fmts[0];
            c->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;

            // some formats want stream headers to be separate
            if ((oc->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0)
            {
                c->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
            }

            return st;
        }
        /// <summary>
        /// PrepareResampler
        /// </summary>
        private void PrepareResampler()
        {
            //Resampler
            //ffmpeg.av_frame_copy_props(SwrFrame, Frame);
            //SwrFrame->nb_samples = Codec->frame_size;
            //SwrFrame->format = (int)Codec->sample_fmt;
            //SwrFrame->channel_layout = Codec->channel_layout;
            //SwrFrame->channels = Codec->channels;
            //SwrFrame->sample_rate = Codec->sample_rate;
            //SwrFrame->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
            //SwrFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
            //SwrFrame->sample_rate = Frame->sample_rate;
            //SwrFrame->channels = 2;
            //if (SwrFrame->nb_samples <= 0) SwrFrame->nb_samples = 64;
            //Swr = ffmpeg.swr_alloc();
            //if (Swr == null)
            //    die("SWR = Null");
            //Ret = ffmpeg.swr_config_frame(Swr, SwrFrame, Frame);
            //CheckRet();
            //Ret = ffmpeg.swr_init(Swr);
            //CheckRet();
            //Ret = ffmpeg.swr_is_initialized(Swr);
            //CheckRet();

            //Encoder
            //string outfile = "a.wav";
            //AVOutputFormat* fmt = fmt = ffmpeg.av_guess_format(null, outfile, null); //ffmpeg.av_guess_format("s16le", null, null);
            //if (fmt == null) die("av_guess_format");
            //OutFormat = ffmpeg.avformat_alloc_context();
            //OutFormat->oformat = fmt;
            //out_audioStream = add_audio_stream(OutFormat, fmt->audio_codec);
            //OutCodec = open_audio(OutFormat, out_audioStream);
            //out_audioStream->time_base = Stream->time_base;
            //Ret = ffmpeg.avio_open2(&OutFormat->pb, outfile, ffmpeg.AVIO_FLAG_WRITE, null, null);
            //CheckRet();
            //ffmpeg.avformat_write_header(OutFormat, null);
            //AVCodec* ocodec;
            //Ret = ffmpeg.av_find_best_stream(OutFormat, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &ocodec, 0);
            //CheckRet();
            //OutCodec = out_audioStream->codec;
            //OutCodec->time_base.num = 1;
            //OutCodec->time_base.den = 1000;
            //AVOutputFormat* fmt = fmt = ffmpeg.av_guess_format(null, Outfile, null);
            AVOutputFormat* fmt = fmt = ffmpeg.av_guess_format("s16le", null, null);
            if (fmt == null)
            {
                die("av_guess_format");
            }

            OutFormat = ffmpeg.avformat_alloc_context();
            OutFormat->oformat = fmt;
            out_audioStream = add_audio_stream(OutFormat, fmt->audio_codec);
            open_audio(OutFormat, out_audioStream);
            out_audioStream->time_base = Stream->time_base;
            Ret = ffmpeg.avio_open2(&OutFormat->pb, Outfile, ffmpeg.AVIO_FLAG_WRITE, null, null);
            if (Ret < 0)
            {
                die("url_fopen");
            }

            ffmpeg.avformat_write_header(OutFormat, null);
            AVCodec* ocodec;
            Ret = ffmpeg.av_find_best_stream(OutFormat, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &ocodec, 0);
            OutCodec = ffmpeg.avcodec_alloc_context3(ocodec);
            //OutCodec = out_audioStream->codec;
            ffmpeg.avcodec_parameters_to_context(OutCodec, out_audioStream->codecpar);
            Ret = ffmpeg.avcodec_open2(OutCodec, ocodec, null);
            if (Ret < 0)
            {
                die("avcodec_open2");
            }

            Swr = ffmpeg.swr_alloc();
            ffmpeg.av_opt_set_channel_layout(Swr, "in_channel_layout", (long)Codec->channel_layout, 0);
            ffmpeg.av_opt_set_channel_layout(Swr, "out_channel_layout", (long)OutCodec->channel_layout, 0);
            ffmpeg.av_opt_set_int(Swr, "in_sample_rate", Codec->sample_rate, 0);
            ffmpeg.av_opt_set_int(Swr, "out_sample_rate", OutCodec->sample_rate, 0);
            ffmpeg.av_opt_set_sample_fmt(Swr, "in_sample_fmt", Codec->sample_fmt, 0);
            ffmpeg.av_opt_set_sample_fmt(Swr, "out_sample_fmt", OutCodec->sample_fmt, 0);
            Ret = ffmpeg.swr_init(Swr);
            if (Ret < 0)
            {
                die("swr_init");
            }

            //AVFrame* Frame = ffmpeg.av_frame_alloc();
            //if (Frame == null)
            //    die("Could not allocate audio frame");

            Frame->format = (int)Codec->sample_fmt;
            Frame->channel_layout = Codec->channel_layout;
            Frame->channels = Codec->channels;
            Frame->sample_rate = Codec->sample_rate;

            //AVFrame* SwrFrame = ffmpeg.av_frame_alloc();
            //if (SwrFrame == null) die("Could not allocate audio frame");

            SwrFrame->nb_samples = OutCodec->frame_size;
            SwrFrame->format = (int)OutCodec->sample_fmt;
            SwrFrame->channel_layout = OutCodec->channel_layout;
            SwrFrame->channels = OutCodec->channels;
            SwrFrame->sample_rate = OutCodec->sample_rate;
            if (SwrFrame->nb_samples <= 0)
            {
                SwrFrame->nb_samples = 32; //wave files have a 0 frame_size set I just picked 64 might beable to be bigger.
            }

            AVPacket Packet;
            ffmpeg.av_init_packet(&Packet);
            Packet.data = null;
            Packet.size = 0;
        }
        //unsafe private static AVCodecContext* open_audio(AVFormatContext* oc, AVStream* st)
        //{
        //    AVCodecContext* c = st->codec;
        //    AVCodec* codec;

        //    /* find the audio encoder */
        //    codec = ffmpeg.avcodec_find_encoder(c->codec_id);
        //    if (codec == null) die("avcodec_find_encoder");

        //    /* open it */
        //    AVDictionary* dict = null;
        //    ffmpeg.av_dict_set(&dict, "strict", "+experimental", 0);
        //    ffmpeg.avcodec_alloc_context3(codec);
        //    ffmpeg.avcodec_parameters_to_context(c, st->codecpar);
        //    int res = ffmpeg.avcodec_open2(c, codec, &dict);
        //    if (res < 0) die("avcodec_open");
        //    return c;
        //}
        unsafe private static void open_audio(AVFormatContext* oc, AVStream* st)
        {
            AVCodecContext* c = st->codec;
            AVCodec* codec;

            /* find the audio encoder */
            codec = ffmpeg.avcodec_find_encoder(c->codec_id);
            if (codec == null)
            {
                die("avcodec_find_encoder");
            }

            /* open it */
            AVDictionary* dict = null;
            ffmpeg.av_dict_set(&dict, "strict", "+experimental", 0);
            int res = ffmpeg.avcodec_open2(c, codec, &dict);
            if (res < 0)
            {
                die("avcodec_open");
            }
        }
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
            byte[] errbuff = new byte[256];
            fixed (byte* ptr = &errbuff[0])
            {
                ffmpeg.av_strerror(ret, ptr, errbuff_size);
            }

            return Encoding.UTF8.GetString(errbuff);
        }
        //I think this might work for raw data but i havn't tested it.
        //private void Parse()
        //{
        //    try
        //    {
        //        using (FileStream fs = File.OpenRead(File_Name))
        //        {
        //            int MAX = 4096;
        //            byte[] data = new byte[MAX];
        //            long remain = fs.Length;
        //            long count = 0;
        //            while (remain > 0)
        //            {
        //                /* read raw data from the input file */
        //                //data_size = fread(inbuf, 1, INBUF_SIZE, f);
        //                //if (!data_size)
        //                //    break;
        //                int read = fs.Read(data, (int)count, MAX);

        //                /* use the parser to split the data into frames */
        //                while (read > 0)
        //                {
        //                    fixed (byte* t = &data[0])
        //                    {
        //                        Ret = ffmpeg.av_parser_parse2(Parser, Codec, &Packet->data, &Packet->size,
        //                            t, read, ffmpeg.AV_NOPTS_VALUE, ffmpeg.AV_NOPTS_VALUE, 0);
        //                    }
        //                        CheckRet();
        //                    remain -= Ret;
        //                    read -= Ret;
        //                    count += Ret;
        //                    if (remain < MAX) MAX = (int)remain;

        //                    if (Packet->size > 0)
        //                        Decode();//(c, frame, pkt, outfilename);
        //                }
        //            }
        //        }
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        // Insert some logic here
        //        throw (ex);
        //    }
        //    catch (FileNotFoundException ex)
        //    {
        //        // Insert some logic here
        //        throw (ex);
        //    }
        //    catch (IOException ex)
        //    {
        //        // Insert some logic here
        //        throw (ex);
        //    }
        //}
        private int WritetoMs(byte* output, int start, int length)
        {
            long c_len = Ms.Length;
            for (int i = start; i < length; i++)
            {
                Ms.WriteByte(output[i]);
            }
            if (Ms.Length - c_len != length)
            {
                die("not all data wrote");
            }

            return (int)(Ms.Length - c_len);
        }
        /// <summary>
        /// Decode loop. Will keep grabbing frames of data till an error or end of file.
        /// </summary>
        private void Decode()
        {

            Ret = ffmpeg.avcodec_send_packet(Codec, Packet);
            if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
            {
                // The decoder doesn't have enough data to produce a frame
                return;
            }

            if (Packet->stream_index != Stream_index)
            {
                return; // wrong stream.
            }

            if (Ret == ffmpeg.AVERROR_EOF)
            {
                return;
            }
            else
            {
                CheckRet();
            }

            while (Ret >= 0)
            {
                Ret = ffmpeg.avcodec_receive_frame(Codec, Frame);
                if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    // The decoder doesn't have enough data to produce a frame
                    return;
                }
                else if (Ret == ffmpeg.AVERROR_EOF)
                {
                    // End of file..
                    return;
                }
                else
                {
                    CheckRet();
                }

                if (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                {
                    // do something with video here.
                    BMP_Save();
                }
                else if (Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    // do something with audio here.
                    // Adapted from https://libav.org/documentation/doxygen/master/decode_audio_8c-example.html
                    /* the stream parameters may change at any time, check that they are
                    * what we expect */
                    Channels = ffmpeg.av_get_channel_layout_nb_channels(Frame->channel_layout);
                    /* The decoded data is signed 16-bit planar -- each channel in its own
                     * buffer. We interleave the two channels manually here, but using
                     * libavresample is recommended instead. */
                    if (Channels == 2 && Frame->format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16P)
                    {
                        byte*[] tmp = Frame->data;
                        for (int i = 0; i < Frame->linesize[0]; i++)
                        {
                            Ms.WriteByte(tmp[0][i]);
                            Ms.WriteByte(tmp[1][i]);
                        }
                    }
                    else if (Frame->format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16)
                    {
                        byte*[] tmp = Frame->data;
                        for (int i = 0; i < Frame->linesize[0]; i++)
                        {
                            Ms.WriteByte(tmp[0][i]);
                        }
                    }
                    // must resample
                    else
                    {
                        Resample();
                    }
                }
            }
        }
        private void Resample()
        {
            if (Codec->frame_number == 1)
            {
                PrepareResampler();
            }

            int frameFinished = 0;
            // Convert
            byte* convertedData = null;

            if (ffmpeg.av_samples_alloc(&convertedData,
                         null,
                         OutCodec->channels,
                         SwrFrame->nb_samples,
                         OutCodec->sample_fmt, 0) < 0)
            {
                die("Could not allocate samples");
            }

            int outSamples = 0;
            fixed (byte** tmp = (byte*[])Frame->data)
            {
                outSamples = ffmpeg.swr_convert(Swr, null, 0,
                             //&convertedData,
                             //SwrFrame->nb_samples,
                             tmp,
                     Frame->nb_samples);
            }
            if (outSamples < 0)
            {
                die("Could not convert");
            }
            //int lastOutsamples = 0;
            for (; ; )
            {
                int nb_samples = SwrFrame->nb_samples;
                outSamples = ffmpeg.swr_get_out_samples(Swr, 0);
                if ((outSamples < OutCodec->frame_size * OutCodec->channels) || OutCodec->frame_size == 0 && (outSamples < SwrFrame->nb_samples * OutCodec->channels))
                {
                    break;
                }
                //if ((outSamples < OutCodec->frame_size * OutCodec->channels) || (OutCodec->frame_size == 0 && (outSamples <= 0 || (outSamples < nb_samples && lastOutsamples == outSamples)))) break; // see comments, thanks to @dajuric for fixing this
                //lastOutsamples = outSamples;
                outSamples = ffmpeg.swr_convert(Swr,
                                         &convertedData,
                                         nb_samples, null, 0);
                //if (outSamples != 0) lastOutsamples = 0; //had issue with one file having 36 samples left and stuck in loop. i think was from changing sample rate
                int buffer_size = ffmpeg.av_samples_get_buffer_size(null,
                               OutCodec->channels,
                               nb_samples,
                               OutCodec->sample_fmt,
                               0);
                if (buffer_size < 0)
                {
                    die("Invalid buffer size");
                }

                if (ffmpeg.avcodec_fill_audio_frame(SwrFrame,
                         OutCodec->channels,
                         OutCodec->sample_fmt,
                         convertedData,
                         buffer_size,
                         0) < 0)
                {
                    die("Could not fill frame");
                }

                AVPacket outPacket;
                ffmpeg.av_init_packet(&outPacket);
                outPacket.data = null;
                outPacket.size = 0;

                if (ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished) < 0) // old api once i get this working need to update to new api
                {
                    die("Error encoding audio frame");
                }
                //if (audioStream->codec->coded_frame != null && audioStream->codec->coded_frame->pts != (int)ffmpeg.AV_NOPTS_VALUE)
                //    outPacket.pts = ffmpeg.av_rescale_q(audioStream->codec->coded_frame->pts,
                //        audioStream->codec->time_base, audioStream->codec->time_base);


                //outPacket.flags |= ffmpeg.AV_PKT_FLAG_KEY;
                outPacket.stream_index = out_audioStream->index;
                //outPacket.data = audio_outbuf;
                outPacket.dts = Frame->pkt_dts;
                outPacket.pts = Frame->pkt_pts;
                ffmpeg.av_packet_rescale_ts(&outPacket, Stream->time_base, out_audioStream->time_base);

                if (frameFinished != 0)
                {


                    if ((Ret = ffmpeg.av_interleaved_write_frame(OutFormat, &outPacket)) != 0)
                    {
                        CheckRet(); //die("Error while writing audio frame");
                    }

                    ffmpeg.av_free_packet(&outPacket);
                }
            }

        }

        private void Resample3()//broken
        {
            if (Codec->frame_number == 1)
            {
                PrepareResampler();
            }

            Channels = ffmpeg.av_get_channel_layout_nb_channels(SwrFrame->channel_layout);
            int nb_channels = ffmpeg.av_get_channel_layout_nb_channels(SwrFrame->channel_layout);
            int bytes_per_sample = ffmpeg.av_get_bytes_per_sample(AVSampleFormat.AV_SAMPLE_FMT_S16) * nb_channels;
            bool got_packet = false;
            if ((Ret = ffmpeg.swr_convert_frame(Swr, SwrFrame, Frame)) >= 0)
            {
                WritetoMs(*SwrFrame->extended_data, 0, SwrFrame->nb_samples * bytes_per_sample);

                //Encode(SwrFrame, ref got_packet);
            }
            else
            {
                CheckRet();
            }
        }
        private void Resample2() //broken
        {
            if (Codec->frame_number == 1)
            {
                PrepareResampler();
            }

            int nb_samples = Frame->nb_samples;
            int output_nb_samples = nb_samples;
            int nb_channels = ffmpeg.av_get_channel_layout_nb_channels(ffmpeg.AV_CH_LAYOUT_STEREO);
            int bytes_per_sample = ffmpeg.av_get_bytes_per_sample(AVSampleFormat.AV_SAMPLE_FMT_S16) * nb_channels;
            int bufsize = ffmpeg.av_samples_get_buffer_size(null, nb_channels, nb_samples,
                                                     AVSampleFormat.AV_SAMPLE_FMT_S16, 1);

            byte*[] b = Frame->data;
            fixed (byte** input = b)
            {
                byte* output = null;
                ffmpeg.av_samples_alloc(&output, null,
                    nb_channels,
                    nb_samples,
                    (AVSampleFormat)Frame->format, 0);//

                // Buffer input

                Ret = ffmpeg.swr_convert(Swr, &output, output_nb_samples / 2, input, nb_samples);
                CheckRet();
                FillFrame(output, Ret * bytes_per_sample);
                WritetoMs(output, 0, Ret * bytes_per_sample);
                bool got_packet = false;
                //Encode(SwrFrame, ref got_packet);
                output_nb_samples -= Ret;

                // Drain buffer
                while ((Ret = ffmpeg.swr_convert(Swr, &output, output_nb_samples, null, 0)) > 0)
                {
                    CheckRet();
                    FillFrame(output, Ret * bytes_per_sample);
                    WritetoMs(output, 0, Ret * bytes_per_sample);
                    got_packet = false;
                    //Encode(SwrFrame, ref got_packet);
                    output_nb_samples -= Ret;
                }
            }
        }
        private void FillFrame(byte* convertedData, int buffer_size) // broken
        {

            Ret = ffmpeg.avcodec_fill_audio_frame(SwrFrame,
                     OutCodec->channels,
                     OutCodec->sample_fmt,
                     convertedData,
                     buffer_size,
                     0);
            CheckRet();
        }
        private int Encode(AVFrame* OutFrame, ref bool got_packet)
        {
            AVPacket OutPacket;
            ffmpeg.av_init_packet(&OutPacket);
            OutPacket.data = null;
            OutPacket.size = 0;

            got_packet = false;

            Ret = ffmpeg.avcodec_send_frame(OutCodec, OutFrame);
            CheckRet();

            for (; ; )
            {
                Ret = ffmpeg.avcodec_receive_packet(OutCodec, &OutPacket);
                if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                {
                    return 0; //might need to return Ret encase we need to check for eagain?
                }
                else if (CheckRet() >= 0)
                {
                    got_packet = true;
                }


                //OutPacket.flags |= ffmpeg.AV_PKT_FLAG_KEY;
                //OutPacket.data = audio_outbuf;
                //OutPacket.dts = OutFrame->pkt_dts;
                //OutPacket.pts = OutFrame->pkt_pts;

                if (got_packet)
                {
                    //ffmpeg.av_packet_rescale_ts(&OutPacket, Stream->time_base, out_audioStream->time_base);

                    //OutPacket.stream_index = out_audioStream->index;

                    Ret = ffmpeg.av_interleaved_write_frame(OutFormat, &OutPacket);
                    CheckRet();
                    ffmpeg.av_packet_unref(&OutPacket);
                }
            }
            return Ret;
        }
        private int ReadOne()
        {
            Ret = ffmpeg.av_read_frame(Format, Packet);
            if (Ret == ffmpeg.AVERROR_EOF)
            {
                return Update(FfccState.DONE, Ret);
            }
            CheckRet();
            return Update(FfccState.PACKET, Ret);
        }
        private int GetPacket()
        {
            Ret = ffmpeg.avcodec_send_packet(Codec, Packet);
            if (Packet->stream_index != Stream_index)
            {
                return Update(FfccState.READONE); // wrong stream.
            }

            if (Ret == ffmpeg.AVERROR_EOF)
            {
                return Update(FfccState.DONE, Ret);
            }
            else
            {
                CheckRet();
            }

            return Update(FfccState.FRAME, Ret);
        }
        public int GetFrame()
        {
            Ret = 0;
            if (Packet->size == 0)
            {
                return Update(FfccState.READONE);
            }

            Ret = ffmpeg.avcodec_receive_frame(Codec, Frame);
            if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN) || (FrameSkip && Behind()))
            {
                // The decoder doesn't have enough data to produce a frame
                return Update(FfccState.READONE);
            }
            else if (Ret == ffmpeg.AVERROR_EOF)
            {
                // End of file..
                return Update(FfccState.DONE, Ret);
            }
            else
            {
                CheckRet();
            }

            return Update(FfccState.WAITING, Ret);
        }
        /// <summary>
        /// Saves each frame to it's own BMP image file.
        /// </summary>
        private void BMP_Save()//byte* buf, int wrap, int xsize, int ysize,       filename                   )
        {
            string filename = $"{Path.GetFileNameWithoutExtension(File_Name)}_rawframe.{Codec->frame_number}.bmp";
            using (FileStream fs = File.OpenWrite(filename))
            {
                Bitmap bitmap = FrameToBMP();
                bitmap.Save(fs, ImageFormat.Bmp);

                //int i;

                //f=fopen(filename,"w");
                //fprintf(f, "P5\n%d %d\n%d\n", xsize, ysize, 255);

                //my attempt to convert the fwrite code to c#.
                //for (i = 0; i < ysize; i++)
                //{
                //    //c++ woot
                //    //fwrite(buf + i * wrap, 1, xsize, f);
                //    //alt version was maybe
                //    ////byte[] arr = new byte[xsize];
                //    ////Marshal.Copy((IntPtr)buf, arr, i * wrap, xsize);
                //    ////fs.Write(arr, 0, xsize);
                //    //or this
                //    //I thought it'd be safer to write one byte at a time
                //    int y = i * wrap;
                //    for (int x = 0; x < xsize; x++)
                //        fs.WriteByte(buf[x + y]);
                //}

                //fclose(f);
            }
        }
        /// <summary>
        /// Converts raw video frame to correct colorspace
        /// </summary>
        /// <remarks>AForge source for refernce this function. was c++.</remarks>
        /// <returns>Bitmap of frame</returns>
        public Bitmap FrameToBMP()
        {
            AVPixelFormat v = Codec->pix_fmt;
            Bitmap bitmap = new Bitmap(Codec->width, Codec->height, PixelFormat.Format32bppArgb);

            // lock the bitmap
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, Codec->width, Codec->height),
            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte* ptr = (byte*)(bitmapData.Scan0);

            byte*[] srcData = { ptr, null, null, null };
            int[] srcLinesize = { bitmapData.Stride, 0, 0, 0 };

            // convert video frame to the RGB bitmap
            ffmpeg.sws_scale(Sws, Frame->data, Frame->linesize, 0, Codec->height, srcData, srcLinesize);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }
        /// <summary>
        /// Converts BMP to Texture
        /// </summary>
        /// <returns>Texture2D</returns>
        public Texture2D FrameToTexture2D()
        {
            //Get Bitmap. there might be a way to skip this step.
            Bitmap frame = FrameToBMP();
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
}
