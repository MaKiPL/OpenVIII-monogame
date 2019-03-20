using FFmpeg.AutoGen;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



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
        private AVFrame* _outFrame;

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
        public AVFrame* SwrFrame { get => _outFrame; private set => _outFrame = value; }
        FfccState State { get; set; }
        FfccMode Mode { get; set; }
        public int FPS { get => (Codec != null ? (Codec->framerate.den == 0 || Codec->framerate.num == 0 ? 0 : Codec->framerate.num / Codec->framerate.den) : 0); }
        public SoundEffectInstance see { get; private set; }
        private SoundEffect se { get; set; }
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
                throw new Exception("Class not Init");
            if (state == FfccState.NODLL) return -1;
            if (state != FfccState.NULL) State = state;
            do
            {
                switch (State)
                {
                    case FfccState.INIT:
                        Init();
                        if (Mode == FfccMode.PROCESS_ALL)
                            State = FfccState.READALL;
                        else State = FfccState.WAITING;
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
        private void ReadAll()
        {

            Ms = new ManualMemoryStream();

            while (true)
            {
                Ret = ffmpeg.av_read_frame(Format, Packet);
                if (Ret == ffmpeg.AVERROR_EOF)
                    break;
                CheckRet();
                Decode();
            }
            if (Ms.Length > 0)
            {
                try
                {
                    // accepts streams with s16le wave files maybe more haven't tested everything.
                    se = SoundEffect.FromStream(Ms); 
                }
                catch (ArgumentException)
                {
                    // accepts s16le maybe more haven't tested everything.
                    se = new SoundEffect(Ms.ToArray(), 0, (int)Ms.Length, SwrFrame->sample_rate, (AudioChannels)Channels, 0, 0);
                }
                see = se.CreateInstance();
                see.Play();
            }

            Ms.ManualDispose();
        }
        ~Ffcc()
        {
            try
            {
                fixed (AVFrame** tmp = &_frame)
                    ffmpeg.av_frame_free(tmp);
                fixed (AVFrame** tmp = &_outFrame)
                    ffmpeg.av_frame_free(tmp);
                fixed (AVPacket** tmp = &_packet)
                    ffmpeg.av_packet_free(tmp);
                fixed (SwrContext** tmp = &_swr)
                    ffmpeg.swr_free(tmp);
                ffmpeg.av_parser_close(Parser);
                ffmpeg.avcodec_close(Codec);
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
                    Ret = ffmpeg.avformat_open_input(tmp, File_Name, null, null);
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
        private void CheckRet()
        {
            switch (Ret)
            {
                case ffmpeg.AVERROR_OUTPUT_CHANGED:
                    throw new Exception($"The swr_context output ch_layout, sample_rate, sample_fmt must match outframe! {Ret} - {AvError(Ret)}");
                case ffmpeg.AVERROR_INPUT_CHANGED:
                    throw new Exception($"The swr_context input ch_layout, sample_rate, sample_fmt must match inframe! {Ret} - {AvError(Ret)}");
                default:
                    if (Ret < 0)
                        throw new Exception($"{Ret} - {AvError(Ret)}");
                    return;
            }
        }
        /// <summary>
        /// Init Class
        /// </summary>
        private void Init()
        {
            //default values below here.
            Ret = -1;
            try
            {
                _format = ffmpeg.avformat_alloc_context();
                //Swr = ffmpeg.swr_alloc();
                Parser = null;
                Codec = null;
                Stream = null;
                Packet = ffmpeg.av_packet_alloc();
                if (Packet == null)
                    throw new Exception("Error allocating the packet\n");
                //ffmpeg.av_init_packet(Packet);
                SwrFrame = ffmpeg.av_frame_alloc();
                if (SwrFrame == null)
                    throw new Exception("Error allocating the frame\n");
                Frame = ffmpeg.av_frame_alloc();
                if (Frame == null)
                    throw new Exception("Error allocating the frame\n");
                Stream_index = -1;
                Channels = 0;
                Sample_rate = 44100;

                //ffmpeg.av_register_all(); //should not need this.


                if (Open() < 0)
                    throw new Exception("No file not open");


                Get_Stream();
                FindOpenCodec();
                Get_Stream();
                if (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                    PrepareSWS();
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
                throw new Exception($"Could not retrieve {(Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO ? "audio" : (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO ? "video" : "other"))} stream from file \n {File_Name}");
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
                throw new Exception("Codec not found");
            //Parser = ffmpeg.av_parser_init((int)c->id);
            //if (Parser == null)
            //    throw new Exception("parser not found");
            //Commented due to Stream->codec doing the work for it.
            //Codec = ffmpeg.avcodec_alloc_context3(c);
            //if (Codec == null)
            //    throw new Exception("Could not allocate video codec context");
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
        /// <summary>
        /// PrepareResampler
        /// </summary>
        private void PrepareResampler()
        {
            ffmpeg.av_frame_copy_props(SwrFrame, Frame);
            SwrFrame->channel_layout = ffmpeg.AV_CH_LAYOUT_STEREO;
            SwrFrame->format = (int)AVSampleFormat.AV_SAMPLE_FMT_S16;
            SwrFrame->sample_rate = Frame->sample_rate;
            SwrFrame->channels = 2;
            Swr = ffmpeg.swr_alloc();
            if (Swr == null)
                throw new Exception("SWR = Null");
            Ret = ffmpeg.swr_config_frame(Swr, SwrFrame, Frame);
            CheckRet();
            Ret = ffmpeg.swr_init(Swr);
            CheckRet();
            Ret = ffmpeg.swr_is_initialized(Swr);
            CheckRet();
        }
        /// <summary>
        /// Converts FFMPEG error codes into a string.
        /// </summary>
        private string AvError(int ret)
        {

            ulong errbuff_size = 256;
            byte[] errbuff = new byte[256];
            fixed (byte* ptr = &errbuff[0])
                ffmpeg.av_strerror(ret, ptr, errbuff_size);
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
        private int WritetoMs(byte * output,int start, int length)
        {
            long c_len = Ms.Length;
            for (int i = start; i < length; i++)
            {
                Ms.WriteByte(output[i]);
            }
            if (Ms.Length - c_len != length)
                throw new Exception("not all data wrote");
            return (int)(Ms.Length - c_len);
        }
        /// <summary>
        /// Decode loop. Will keep grabbing frames of data till an error or end of file.
        /// </summary>
        private void Decode()
        {

            Ret = ffmpeg.avcodec_send_packet(Codec, Packet);
            if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                // The decoder doesn't have enough data to produce a frame
                return;
            if (Packet->stream_index != Stream_index) return; // wrong stream.
            if (Ret == ffmpeg.AVERROR_EOF) return;
            else
                CheckRet();

            while (Ret >= 0)
            {
                Ret = ffmpeg.avcodec_receive_frame(Codec, Frame);
                if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                    // The decoder doesn't have enough data to produce a frame
                    return;
                else if (Ret == ffmpeg.AVERROR_EOF)
                    // End of file..
                    return;
                else CheckRet();

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
                PrepareResampler();
            Channels = ffmpeg.av_get_channel_layout_nb_channels(SwrFrame->channel_layout);
            int nb_channels = ffmpeg.av_get_channel_layout_nb_channels(SwrFrame->channel_layout);
            int bytes_per_sample = ffmpeg.av_get_bytes_per_sample(AVSampleFormat.AV_SAMPLE_FMT_S16) * nb_channels;

            if ((Ret = ffmpeg.swr_convert_frame(Swr, SwrFrame, Frame)) >= 0)
                //WritetoMs(*OutFrame->extended_data, 0, OutFrame->nb_samples * bytes_per_sample);
            CheckRet();
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
            if (Packet->stream_index != Stream_index) return Update(FfccState.READONE); // wrong stream.
            if (Ret == ffmpeg.AVERROR_EOF) return Update(FfccState.DONE, Ret);
            else
                CheckRet();
            return Update(FfccState.FRAME, Ret);
        }
        public int GetFrame()
        {
            Ret = 0;
            if (Packet->size == 0) return Update(FfccState.READONE);

            Ret = ffmpeg.avcodec_receive_frame(Codec, Frame);
            if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                // The decoder doesn't have enough data to produce a frame
                return Update(FfccState.READONE);
            else if (Ret == ffmpeg.AVERROR_EOF)
                // End of file..
                return Update(FfccState.DONE, Ret);
            else CheckRet();
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
            var v = Codec->pix_fmt;
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
