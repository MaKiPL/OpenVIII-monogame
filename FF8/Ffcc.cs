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
        /// <summary>
        /// Method of Format, required for atleast one function.
        /// </summary>
        //public AVFormatContext* _decodeFormatContext; // FormatContext
        //private AVFrame* _Decoder.Frame;
        //private AVPacket* _Decoder.Packet;
        private SwrContext* _resampleContext;
        private SwsContext* _scalerContext;
        private AVFrame* _resampleFrame;
        private AVCodecContext* _decodeCodecContext;
        private FileStream _decodeFileStream;
        private AVCodecParserContext* _parserContext;

        private FfccVaribleGroup Decoder { get; set; } = new FfccVaribleGroup();
        /// <summary>
        /// Most ffmpeg functions return an integer. If the value is less than 0 it is an error usually. Sometimes data is passed and then it will be greater than 0.
        /// </summary>
        private int Return { get; set; }


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
        private AVCodecParserContext* ParserContext { get => _parserContext; set => _parserContext = value; }

        /// <summary>
        /// Codec context, set from stream
        /// </summary>
        private AVCodecContext* DecodeCodecContext { get => _decodeCodecContext; set => _decodeCodecContext = value; }
        public AVCodec* DecodeCodec { get; private set; }

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
        private MemoryStream DecodedStream { get; set; }
        public AVFrame* ResampleFrame { get => _resampleFrame; private set => _resampleFrame = value; }
        private FfccState State { get; set; }
        private FfccMode Mode { get; set; }
        private bool useduration => DecodeCodecContext == null || (DecodeCodecContext->framerate.den == 0 || DecodeCodecContext->framerate.num == 0);
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
                else if (DecodeCodecContext != null && DecodeCodecContext->framerate.den != 0)
                {
                    return DecodeCodecContext->framerate.num / (double)DecodeCodecContext->framerate.den;
                }

                return 0;
            }
        }

        /// <summary>
        /// if there is no stream it returns false. only checked when trying to process audio
        /// </summary>
        private bool AudioEnabled => DecoderStreamIndex >= 0;
        public SoundEffectInstance SoundInstance { get; private set; }
        private SoundEffect Sound { get; set; }
        public AVCodecContext* EncoderCodecContext { get; private set; }
        public AVFormatContext* EncoderFormatContext { get; private set; }
        public string EncodedFileName { get; private set; }
        public string EncodedFileType { get; private set; }
        public bool FrameSkip { get; set; } = true; // when getting video frames if behind it goes to next frame.
        /// <summary>
        /// Stopwatch tracks the time audio has played so video can sync or loops can be looped.
        /// </summary>
        public Stopwatch timer { get; } = new Stopwatch();
        public FileStream DecodeFileStream { get => _decodeFileStream; set => _decodeFileStream = value; }
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
            /// Get frames from packet4
            /// </summary>
            NODLL,
            GETSTREAM,
            GETCODEC,
            PREPARE_SWS,
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
            //Parse(); //doesn't seem to work. Or it only works with raw streams.
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
                        State = FfccState.GETSTREAM;
                        Init();
                        break;
                    case FfccState.GETSTREAM:
                        State = FfccState.GETCODEC;
                        Get_Stream();
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
                        //return ReadOne();
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

                if (SoundInstance != null && !SoundInstance.IsDisposed && AudioEnabled)
                {
                    SoundInstance.Play();
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
            if (SoundInstance != null && !SoundInstance.IsDisposed && AudioEnabled)
            {
                SoundInstance.Stop();
            }
            if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
            {
                dsee44100.Stop();
            }

            if (dsee44100 != null && !dsee44100.IsDisposed && AudioEnabled)
            {
                dsee48000.Stop();
            }

            if (Sound != null && !Sound.IsDisposed)
            {
                Sound.Dispose();
            }

            if (SoundInstance != null && !SoundInstance.IsDisposed)
            {
                SoundInstance.Dispose();
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
        private bool SoundExistsAndReady()
        {
            // I'm not sure how to write this better.
            //Outfile = Path.Combine(Path.GetDirectoryName(Outfile), $"{Path.GetFileNameWithoutExtension(Outfile)}.pcm");
            //wr = new WaveFileReader(@"C:\eyes_on_me.wav");//Outfile))
            if (File.Exists(EncodedFileName) && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO && Mode == FfccMode.PROCESS_ALL)
            {
                using (DecodeFileStream = File.OpenRead(EncodedFileName))
                {
                    if (DecodeFileStream.Length > 0)
                    {
                        using (DecodedStream = new MemoryStream())
                        {
                            DecodeFileStream.CopyTo(DecodedStream);

                            LoadSoundFromStream();

                            return true;
                        }
                    }
                }
            }

            return false;
        }
        private void LoadSoundFromStream()
        {
            LoadSoundFromStream(DecodedStream);
        }

        private DynamicSoundEffectInstance dsee44100 = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
        private DynamicSoundEffectInstance dsee48000 = new DynamicSoundEffectInstance(48000, AudioChannels.Stereo);
        private int _loopstart = -1;

        public int LOOPSTART { get => _loopstart; private set => _loopstart = value; }
        public void LoadSoundFromStream(MemoryStream decodedStream)
        {
            if (DecodedStream.Length > 0 && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                //    // accepts s16le maybe more haven't tested everything.
                //    Sound = new SoundEffect(DecodedStream.ToArray(), ResampleFrame->sample_rate, (AudioChannels)DecodeCodecContext->channels);
                //    SoundInstance = Sound.CreateInstance();

                if (ResampleFrame->sample_rate == 44100)
                {
                    dsee44100.SubmitBuffer(decodedStream.ToArray());
                }
                else if (ResampleFrame->sample_rate == 48000)
                {
                    dsee48000.SubmitBuffer(decodedStream.ToArray());
                }

                //dsee44100.Play();
                //dsee48000.Play();

            }
        }
        public int ExpectedFrame()
        {
            if (timer.IsRunning)
            {
                TimeSpan ts = timer.Elapsed;
                return (int)Math.Round(ts.TotalMilliseconds * (FPS / 1000));
            }
            return 0;
        }
        public int CurrentFrameNum()
        {
            if (DecodeCodecContext != null)
            {
                return DecodeCodecContext->frame_number;
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
                return CurrentFrameNum() > ExpectedFrame();
            }
            return false;
        }
        private long LOOPLENGTH
        {
            get
            {
                if (LOOPSTART >= 0 && LOOPED)
                {

                    return (DecoderStream->duration - LOOPSTART);
                }
                else
                {
                    return DecoderStream->duration;
                }
            }
        }
        private bool LOOPED = false;
        public bool BehindFrame()
        {
            if (timer.IsRunning && DecoderStreamIndex != -1)
            {
                if (LOOPSTART >= 0 && FPS < 1 && DecoderStream->duration > 0)
                {
                    long time = timer.ElapsedMilliseconds;
                    int secondsbeforeendoftrack = 2;
                    return (time / 1000) > (LOOPLENGTH / DecodeCodecContext->sample_rate) - secondsbeforeendoftrack;
                }
                return CurrentFrameNum() < ExpectedFrame();

            }
            return false;
        }
        public bool CorrectFrame()
        {
            if (timer.IsRunning)
            {
                return CurrentFrameNum() > ExpectedFrame();
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


                    Return = ffmpeg.avcodec_send_packet(DecodeCodecContext, Decoder.Packet);
                    //sent a eof when trying to loop once.
                    //should never get EOF here unless something is wrong.
                    //if (Return == ffmpeg.AVERROR_EOF) 
                    //{
                    //    frame = Decoder.Frame;
                    //    return false;
                    //}
                    //else
                    CheckReturn();

                }
                finally
                {
                    ffmpeg.av_packet_unref(Decoder.Packet);
                }

                Return = ffmpeg.avcodec_receive_frame(DecodeCodecContext, Decoder.Frame);
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
        //public static int EncodeNext(AVCodecContext* avctx, AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        //{
        //    int ret = 0;
        //    got_packet_ptr = 0;
        //    ffmpeg.av_frame_unref(frame);
        //    if ((ret = ffmpeg.avcodec_receive_packet(avctx, avpkt)) == 0)
        //    {
        //        got_packet_ptr = 1;
        //        //0 on success, otherwise negative error code
        //    }
        //    else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
        //    {
        //        //output is not available in the current state - user must try to send input
        //        return Encode(avctx, avpkt, frame, ref got_packet_ptr);
        //    }
        //    else if (ret == ffmpeg.AVERROR_EOF)
        //    {
        //        die("AVERROR_EOF: the encoder has been fully flushed, and there will be no more output packets");
        //    }
        //    else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
        //    {
        //        die("AVERROR(EINVAL) codec not opened, or it is an encoder other errors: legitimate decoding errors");
        //    }
        //    else
        //    {
        //        die("unknown");
        //    }
        //    return ret;//ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished)
        //}
        //public static int Encode(AVCodecContext* avctx, AVPacket* avpkt, AVFrame* frame, ref int got_packet_ptr)
        //{
        //    int ret = 0;
        //    got_packet_ptr = 0;
        //    ffmpeg.av_packet_unref(avpkt);
        //    if ((ret = ffmpeg.avcodec_send_frame(avctx, frame)) == 0)
        //    {
        //        //0 on success, otherwise negative error code
        //        return EncodeNext(avctx, avpkt, frame, ref got_packet_ptr);
        //    }
        //    else if (ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
        //    {
        //        die("input is not accepted in the current state - user must read output with avcodec_receive_packet() (once all output is read, the packet should be resent, and the call will not fail with EAGAIN)");
        //    }
        //    else if (ret == ffmpeg.AVERROR_EOF)
        //    {
        //        die("AVERROR_EOF: the decoder has been flushed, and no new packets can be sent to it (also returned if more than 1 flush packet is sent");
        //    }
        //    else if (ret == ffmpeg.AVERROR(ffmpeg.EINVAL))
        //    {
        //        die("AVERROR(ffmpeg.EINVAL) codec not opened, refcounted_frames not set, it is a decoder, or requires flush");
        //    }
        //    else if (ret == ffmpeg.AVERROR(ffmpeg.ENOMEM))
        //    {
        //        die("AVERROR(ENOMEM) failed to add packet to internal queue, or similar other errors: legitimate decoding errors");
        //    }
        //    else
        //    {
        //        die("unknown");
        //    }
        //    return ret;//ffmpeg.avcodec_encode_audio2(OutCodec, &outPacket, SwrFrame, &frameFinished)
        //}
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
                }
            }
            if (State != FfccState.WAITING)
            {
                State = FfccState.DONE;
                timer.Stop();
                timer.Reset();
                //if (!skipencode)
                //{
                //    EncodeFlush(EncoderCodecContext);
                //}

                DecodeFlush(ref _decodeCodecContext, ref *Decoder.Packet); //calling this twice was causing issues.
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
                    if ((outSamples < (EncoderCodecContext == null ? 0 : EncoderCodecContext->frame_size) * ResampleFrame->channels) || (EncoderCodecContext == null ? 0 : EncoderCodecContext->frame_size) == 0 && (outSamples < ResampleFrame->nb_samples * ResampleFrame->channels))
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


                    if (!skipencode) //encode
                    {
                        //ProcessEncode(ref buffer_size);
                    }
                    else // write to buffer
                    {
                        WritetoMs(ref _convertedData, 0, ref buffer_size);
                    }
                }
            }
        }
        //private void ProcessEncode(ref int buffer_size)
        //{
        //    int frameFinished = 0;

        //    if (buffer_size < 0)
        //    {
        //        die("Invalid buffer size");
        //    }
        //    if (ffmpeg.avcodec_fill_audio_frame(ResampleFrame,
        //                                 EncoderCodecContext->channels,
        //                                 EncoderCodecContext->sample_fmt,
        //                                 ConvertedData,
        //                                 buffer_size,
        //                                 0) < 0)
        //    {
        //        die("Could not fill frame");
        //    }
        //    AVPacket outPacket;
        //    ffmpeg.av_init_packet(&outPacket);
        //    outPacket.data = null;
        //    outPacket.size = 0;
        //    if (Encode(EncoderCodecContext, &outPacket, ResampleFrame, ref frameFinished) < 0)
        //    {
        //        die("Error encoding audio frame");
        //    }

        //    //outPacket.flags |= ffmpeg.AV_PKT_FLAG_KEY;
        //    outPacket.stream_index = EncoderStream->index;
        //    //outPacket.data = audio_outbuf;
        //    outPacket.dts = Decoder.Frame.pkt_dts;
        //    outPacket.pts = Decoder.Frame.pkt_pts;
        //    ffmpeg.av_packet_rescale_ts(&outPacket, DecoderStream->time_base, EncoderStream->time_base);

        //    if (frameFinished != 0)
        //    {


        //        if (ffmpeg.av_interleaved_write_frame(EncoderFormatContext, &outPacket) != 0)
        //        {
        //            die("Error while writing audio frame");
        //        }

        //        ffmpeg.av_packet_unref(&outPacket);
        //    }
        //}

        private void PrepareProcess()
        {
            bool skipencoder = true;
            //if (SoundExistsAndReady())
            //{
            //    return;
            //}

            using (DecodedStream = new MemoryStream())
            {
                Process(skipencoder);
                LoadSoundFromStream();
            }
            if (!skipencoder)
            {
                SoundExistsAndReady();
            }

        }
        ~Ffcc()
        {
#if !_WINDOWS
            try
            {
#endif
            Dispose();




#if !_WINDOWS
            }
            catch (DllNotFoundException e)
            {
                TextWriter errorWriter = Console.Error;
                errorWriter.WriteLine(e.Message);
                errorWriter.WriteLine("Clean up failed, maybe FFmpeg DLLs are missing");
            }
#endif

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

                //this code here will get metadata from formatcontext but the ogg tags aren't there.
                string val = "", key = "";
                AVDictionaryEntry* tag = null;
                //tag = ffmpeg.av_dict_get(_Decoder.Format->metadata, "LOOPSTART", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX);
                while ((tag = ffmpeg.av_dict_get(Decoder.Format->metadata, "", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX)) != null)
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
                    key = "";
                    val = "";
                }

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
            //EncodedFileName = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(DecodedFileName)}.pcm");
            //EncodedFileType = "s16le";
            try
            {
                Decoder.Format = ffmpeg.avformat_alloc_context();
                if (Open() < 0)
                {
                    die("No file not open");
                }

                ConvertedData = null;
                //Swr = ffmpeg.swr_alloc();
                ParserContext = null;
                DecodeCodecContext = null;
                DecoderStream = null;
                Decoder.Packet = ffmpeg.av_packet_alloc();
                Decoder.Frame = ffmpeg.av_frame_alloc();

                DecoderStreamIndex = -1;


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
            DecoderStreamIndex = -1;
            // Find the index of the first audio stream
            for (int i = 0; i < Decoder.Format->nb_streams; i++)
            {
                if (Decoder.Format->streams[i]->codec->codec_type == MediaType)
                {
                    DecoderStreamIndex = i;
                    break; // only grab the first stream
                }
            }
            if (DecoderStreamIndex == -1)
            {
                if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    State = FfccState.DONE;
                    Mode = FfccMode.NOTINIT;
                }
                else
                {
                    die($"Could not retrieve {(MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO ? "audio" : (MediaType == AVMediaType.AVMEDIA_TYPE_VIDEO ? "video" : "other"))} stream from file \n {DecodedFileName}");
                }
            }
            else
            {
                DecoderStream = Decoder.Format->streams[DecoderStreamIndex];

                AVDictionary* metadata = DecoderStream->metadata;//this code here will get metadata from formatcontext but the ogg tags aren't there.
                string val = "", key = "";
                AVDictionaryEntry* tag = null;
                //tag = ffmpeg.av_dict_get(_Decoder.Format->metadata, "LOOPSTART", tag, ffmpeg.AV_DICT_IGNORE_SUFFIX);
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
        }
        public Dictionary<String, String> Metadata { get; private set; } = new Dictionary<string, string>();
        public byte* ConvertedData { get => _convertedData; private set => _convertedData = value; }
        /// <summary>
        /// Finds the codec for the chosen stream
        /// </summary>
        private void FindOpenCodec()
        {
            // find & open codec
            DecodeCodecContext = DecoderStream->codec;
            DecodeCodec = ffmpeg.avcodec_find_decoder(DecodeCodecContext->codec_id);
            if (DecodeCodec == null)
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
            Return = ffmpeg.avcodec_open2(DecodeCodecContext, DecodeCodec, null);
            CheckReturn();
            if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                if (DecodeCodecContext->channel_layout == 0)
                {
                    DecodeCodecContext->channel_layout = ffmpeg.AV_CH_FRONT_LEFT | ffmpeg.AV_CH_FRONT_RIGHT;
                }
            }
        }
        private void PrepareScaler()
        {

            if (MediaType != AVMediaType.AVMEDIA_TYPE_VIDEO)
            {
                return;
            }

            ScalerContext = ffmpeg.sws_getContext(
                DecodeCodecContext->width, DecodeCodecContext->height, DecodeCodecContext->pix_fmt,
                DecodeCodecContext->width, DecodeCodecContext->height, AVPixelFormat.AV_PIX_FMT_RGBA,
                ffmpeg.SWS_ACCURATE_RND, null, null, null);
            Return = ffmpeg.sws_init_context(ScalerContext, null, null);

            CheckReturn();
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
            c->sample_rate = DecoderStream->codec->sample_rate;
            c->channels = DecoderStream->codec->channels;
            c->sample_fmt = AVSampleFormat.AV_SAMPLE_FMT_S16;//encoder->sample_fmts[0];
            c->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;

            // some formats want stream headers to be separate
            if ((oc->oformat->flags & ffmpeg.AVFMT_GLOBALHEADER) != 0)
            {
                c->flags |= ffmpeg.AV_CODEC_FLAG_GLOBAL_HEADER;
            }

            return st;
        }
        //private void PrepareEncoder()
        //{
        //    AVOutputFormat* fmt = fmt = ffmpeg.av_guess_format("s16le", null, null);
        //    if (fmt == null)
        //    {
        //        die("av_guess_format");
        //    }

        //    EncoderFormatContext = ffmpeg.avformat_alloc_context();
        //    EncoderFormatContext->oformat = fmt;
        //    EncoderStream = add_audio_stream(EncoderFormatContext, fmt->audio_codec);
        //    open_audio(EncoderFormatContext, EncoderStream);
        //    EncoderStream->time_base = DecoderStream->time_base;
        //    Return = ffmpeg.avio_open2(&EncoderFormatContext->pb, EncodedFileName, ffmpeg.AVIO_FLAG_WRITE, null, null);
        //    if (Return < 0)
        //    {
        //        die("url_fopen");
        //    }

        //    ffmpeg.avformat_write_header(EncoderFormatContext, null);
        //    AVCodec* ocodec;
        //    Return = ffmpeg.av_find_best_stream(EncoderFormatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, &ocodec, 0);
        //    EncoderCodecContext = ffmpeg.avcodec_alloc_context3(ocodec);
        //    //OutCodec = out_audioStream->codec;
        //    ffmpeg.avcodec_parameters_to_context(EncoderCodecContext, EncoderStream->codecpar);
        //    Return = ffmpeg.avcodec_open2(EncoderCodecContext, ocodec, null);
        //    if (Return < 0)
        //    {
        //        die("avcodec_open2");
        //    }
        //}
        /// <summary>
        /// PrepareResampler
        /// </summary>
        private void PrepareResampler(bool skipencode = true)
        {
            if (MediaType != AVMediaType.AVMEDIA_TYPE_AUDIO)
            {
                return;
            }
            //Encoder
            //if (!skipencode)
            //{
            //    PrepareEncoder();
            //}
            //resampler

            ResampleFrame = ffmpeg.av_frame_alloc();
            if (ResampleFrame == null)
            {
                die("Error allocating the frame\n");
            }
            ResampleContext = ffmpeg.swr_alloc();
            ffmpeg.av_opt_set_channel_layout(ResampleContext, "in_channel_layout", (long)DecodeCodecContext->channel_layout, 0);
            ffmpeg.av_opt_set_int(ResampleContext, "in_sample_rate", DecodeCodecContext->sample_rate, 0);
            ffmpeg.av_opt_set_sample_fmt(ResampleContext, "in_sample_fmt", DecodeCodecContext->sample_fmt, 0);

            ffmpeg.av_opt_set_channel_layout(ResampleContext, "out_channel_layout", EncoderCodecContext == null ? ffmpeg.AV_CH_LAYOUT_STEREO : (long)EncoderCodecContext->channel_layout, 0);
            ffmpeg.av_opt_set_sample_fmt(ResampleContext, "out_sample_fmt", EncoderCodecContext == null ? AVSampleFormat.AV_SAMPLE_FMT_S16 : EncoderCodecContext->sample_fmt, 0);
            ffmpeg.av_opt_set_int(ResampleContext, "out_sample_rate", EncoderCodecContext == null ? DecodeCodecContext->sample_rate : EncoderCodecContext->sample_rate, 0);

            Return = ffmpeg.swr_init(ResampleContext);
            if (Return < 0)
            {
                die("swr_init");
            }
            Decoder.Frame->format = (int)DecodeCodecContext->sample_fmt;
            Decoder.Frame->channel_layout = DecodeCodecContext->channel_layout;
            Decoder.Frame->channels = DecodeCodecContext->channels;
            Decoder.Frame->sample_rate = DecodeCodecContext->sample_rate;

            ResampleFrame->nb_samples = EncoderCodecContext == null || EncoderCodecContext->frame_size == 0 ? 32 : EncoderCodecContext->frame_size;
            ResampleFrame->format = EncoderCodecContext == null ? (int)AVSampleFormat.AV_SAMPLE_FMT_S16 : (int)EncoderCodecContext->sample_fmt;
            ResampleFrame->channel_layout = EncoderCodecContext == null ? ffmpeg.AV_CH_LAYOUT_STEREO : EncoderCodecContext->channel_layout;
            ResampleFrame->channels = EncoderCodecContext == null ? (int)AudioChannels.Stereo : EncoderCodecContext->channels;
            ResampleFrame->sample_rate = EncoderCodecContext == null ? DecodeCodecContext->sample_rate : EncoderCodecContext->sample_rate;

            AVPacket Packet;
            ffmpeg.av_init_packet(&Packet);
            Packet.data = null;
            Packet.size = 0;

            int convertedFrameBufferSize = ffmpeg.av_samples_get_buffer_size(null,
                                                 EncoderCodecContext == null ? ResampleFrame->channels : EncoderCodecContext->channels,
                                                 ResampleFrame->nb_samples,
                                                 EncoderCodecContext == null ? (AVSampleFormat)ResampleFrame->format : EncoderCodecContext->sample_fmt, 0);

            ConvertedData = (byte*)Marshal.AllocHGlobal(convertedFrameBufferSize);
            //if (MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
            //{
            //    fixed(byte** tmp = &_convertedData)
            //        {
            //        if (ffmpeg.av_samples_alloc(tmp,
            //                                     null,
            //                                     EncoderCodecContext == null ? ResampleFrame->channels : EncoderCodecContext->channels,
            //                                     ResampleFrame->nb_samples,
            //                                     EncoderCodecContext == null ? (AVSampleFormat)ResampleFrame->format : EncoderCodecContext->sample_fmt, 0) < 0)
            //        {
            //            die("Could not allocate samples");
            //        }
            //    }
            //}

        }

        private static unsafe void open_audio(AVFormatContext* oc, AVStream* st)
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
        //        using (FileStream fs = File.OpenRead(DecodedFileName))
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
        private int WritetoMs(ref byte* output, int start, ref int length)
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
        /// <summary>
        /// Tests audio against format type to see if can play it raw.
        /// </summary>
        /// <returns>True if decoded raw sample is good
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
            if (DecodeCodecContext->channels == 2 && frame.format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16P)//not tested
            {
                for (int i = 0; i < frame.linesize[0]; i++)
                {
                    DecodedStream.WriteByte(((byte*[])frame.data)[0][i]);
                    DecodedStream.WriteByte(((byte*[])frame.data)[1][i]);
                }
                return true;
            }
            else if (frame.format == (int)AVSampleFormat.AV_SAMPLE_FMT_S16) // played eyes on me wav
            {
                for (int i = 0; i < frame.linesize[0]; i++)
                {
                    DecodedStream.WriteByte(((byte*[])frame.data)[0][i]);
                }
                return true;
            }
            return false;
        }
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
            string filename = Path.Combine(Path.GetTempPath(), $"{Path.GetFileNameWithoutExtension(DecodedFileName)}_rawframe.{DecodeCodecContext->frame_number}.bmp");
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
            Bitmap bitmap=null;
            BitmapData bitmapData=null; 
                
            try
            {
                bitmap = new Bitmap(DecodeCodecContext->width, DecodeCodecContext->height, PixelFormat.Format32bppArgb);
                AVPixelFormat v = DecodeCodecContext->pix_fmt;

                // lock the bitmap
                bitmapData = bitmap.LockBits(new Rectangle(0, 0, DecodeCodecContext->width, DecodeCodecContext->height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                byte * ptr = (byte*)(bitmapData.Scan0);

                byte*[] srcData = { ptr, null, null, null };
                int[] srcLinesize = { bitmapData.Stride, 0, 0, 0 };

                // convert video frame to the RGB bitmap
                ffmpeg.sws_scale(ScalerContext, Decoder.Frame->data, Decoder.Frame->linesize, 0, DecodeCodecContext->height, srcData, srcLinesize);


                return bitmap;
            }
            finally
            {
                if(bitmap!=null && bitmapData!=null)
                bitmap.UnlockBits(bitmapData);
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
        private byte* _convertedData = null;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (DecodedStream != null)
                    {
                        DecodedStream.Dispose();
                    }

                    if (DecodeFileStream != null)
                    {
                        DecodeFileStream.Dispose();
                    }

                    StopSound();
                    // TODO: dispose managed state (managed objects).
                }

                if (ConvertedData != null && MediaType == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {

                    Marshal.FreeHGlobal((IntPtr)ConvertedData);
                    //av_freep throws exceptions no matter where i put it. I think it's linked to ResampleFrame so when i erase that it goes too?
                    //try
                    //    {
                    //   // ffmpeg.av_freep(&ConvertedData[0]);
                    //}
                    //catch { }
                    //try
                    //{
                    //    ffmpeg.av_freep(ConvertedData);
                    //}
                    //catch { }
                }

                if (EncoderFormatContext != null && EncoderFormatContext->pb != null)
                {
                    ffmpeg.avio_close(EncoderFormatContext->pb);
                }
                if (EncoderCodecContext != null)
                {
                    EncodeFlush(EncoderCodecContext);
                    ffmpeg.avcodec_close(EncoderCodecContext);
                    ffmpeg.av_write_trailer(EncoderFormatContext);
                    ffmpeg.avio_close(EncoderFormatContext->pb);
                    ffmpeg.avformat_free_context(EncoderFormatContext);
                }


                //if (DecodeCodecContext != null)
                //{
                //    DecodeFlush(DecodeCodecContext, Decoder.Packet); // was causesing exception flushing happens earlier
                //}

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                ffmpeg.av_frame_unref(ResampleFrame);
                ffmpeg.av_free(ResampleFrame);

                //ffmpeg.av_frame_unref(Decoder.Frame);
                //ffmpeg.av_free(Decoder.Frame);

                //ffmpeg.av_packet_unref(Decoder.Packet);
                //ffmpeg.av_free(Decoder.Packet);


                //ffmpeg.avcodec_close(DecodeCodecContext);
                //ffmpeg.av_free(DecodeCodecContext);
                ffmpeg.av_free(DecodeCodec);

                ffmpeg.sws_freeContext(ScalerContext);
                if (ResampleContext != null)
                {
                    ffmpeg.swr_close(ResampleContext);
                    SwrContext* pResampleContext = ResampleContext;
                    ffmpeg.swr_free(&pResampleContext);
                }
                //if (DecodeFormatContext != null)
                //{
                //    //fixed (AVFormatContext** inputContext = &DecodeFormatContext)
                //    //{
                //    //    //ffmpeg.avformat_close_input(inputContext); //CTD
                //    //}

                //    //ffmpeg.avformat_free_context(DecodeFormatContext);
                //}

                disposedValue = true;


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
