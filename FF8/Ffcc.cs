using FFmpeg.AutoGen;
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
        public enum FfccMode{
            PROCESS_ALL,
            STATE_MACH,
            NOTINIT
        }
        public enum FfccState
            {
            INIT,
            WAITING, //enters waiting state to wait for more input.
            READALL,
            DONE,
            NULL, // used if you want to sometimes pass a value but not always.
            PACKET,
            READONE,
            FRAME
        }
        FfccState State { get; set; }
        FfccMode Mode { get; set; }
        public int FPS { get => (Codec->framerate.den == 0 || Codec->framerate.num == 0 ? 0 : Codec->framerate.num / Codec->framerate.den); }

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
        private int Update(FfccState state = FfccState.NULL,int ret = 0)
        {
            if (Mode == FfccMode.NOTINIT)
                throw new Exception("Class not Init");
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
            while (Mode == FfccMode.PROCESS_ALL && ( State != FfccState.DONE || State != FfccState.WAITING));
            return ret;
        }
        private void ReadAll()
        {
            while (true)
            {
                Ret = ffmpeg.av_read_frame(Format, Packet);
                if (Ret == ffmpeg.AVERROR_EOF) break;
                else if (Ret < 0)
                    CheckRet();
                Decode();
            }
        }
        ~Ffcc()
        {
            fixed (AVFrame** tmp = &_frame)
                ffmpeg.av_frame_free(tmp);
            fixed (AVPacket** tmp = &_packet)
                ffmpeg.av_packet_free(tmp);
            fixed (SwrContext** tmp = &_swr)
                ffmpeg.swr_free(tmp);
            ffmpeg.av_parser_close(Parser);
            ffmpeg.avcodec_close(Codec);
            ffmpeg.avformat_free_context(Format);
        }
        /// <summary>
        /// Opens filename and assigns FormatContext.
        /// </summary>
        private void Open()
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
        }
        /// <summary>
        /// Throws exception if Ret is less than 0
        /// </summary>
        private void CheckRet()
        {
            if (Ret < 0)
                throw new Exception($"{Ret} - {AvError(Ret)}");
        }
        /// <summary>
        /// 
        /// </summary>
        private void Init()
        {
            //default values below here.
            Ret = 0;
            _format = ffmpeg.avformat_alloc_context();
            Swr = ffmpeg.swr_alloc();
            Parser = null;
            Codec = null;
            Stream = null;
            Packet = ffmpeg.av_packet_alloc();
            if (Packet == null)
                throw new Exception("Error allocating the packet\n");
            //ffmpeg.av_init_packet(Packet);
            Frame = ffmpeg.av_frame_alloc();
            if (Frame == null)
                throw new Exception("Error allocating the frame\n");
            Stream_index = -1;
            Channels = 0;
            Sample_rate = 44100;
            //ffmpeg.av_register_all(); //should not need this.
            Open();
            if (!File_Opened)
                throw new Exception("No file not open");
            Get_Stream();
            FindOpenCodec();
            Get_Stream();
            if (Media_Type == AVMediaType.AVMEDIA_TYPE_VIDEO)
                PrepareSWS();
            if (Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                PrepareResampler();


        }
        /// <summary>
        /// 
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
        /// 
        /// </summary>
        private void FindOpenCodec()
        {
            // find & open codec
            Codec = Stream->codec;
            AVCodec* c = ffmpeg.avcodec_find_decoder(Codec->codec_id);
            if (c == null)
                throw new Exception("Codec not found");
            Parser = ffmpeg.av_parser_init((int)c->id);
            if (Parser == null)
                throw new Exception("parser not found");
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
            if(Ret < 0)
                CheckRet();
        }
        /// <summary>
        /// PrepareResampler
        /// </summary>
        private void PrepareResampler()
        {
            // prepare resampler
            ffmpeg.swr_alloc_set_opts(Swr,
                3,
                AVSampleFormat.AV_SAMPLE_FMT_S16,
                Sample_rate,
                (long)Codec->channel_layout,
                Codec->sample_fmt,
                Codec->sample_rate,
                0,
                null);
            if (Swr == null)
                throw new Exception("error: swr_alloc_set_opts()");
            ffmpeg.swr_init(Swr);
            Ret = ffmpeg.swr_is_initialized(Swr);
            if (Ret < 0)
                throw new Exception($"{Ret} - {AvError(Ret)}");
        }
        /// <summary>
        /// Converts FFMPEG error codes into a string.
        /// </summary>
        private static string AvError(int ret)
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
        //                    if (Ret < 0)
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
        /// <summary>
        /// Decode loop. Will keep grabbing frames of data till an error or end of file.
        /// </summary>
        private void Decode()
        {

            Ret = ffmpeg.avcodec_send_packet(Codec, Packet);
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
                    BMP_Save();//Frame->data[0], Frame->linesize[0], Frame->width, Frame->height, outfilename);
                }
                else if (Media_Type == AVMediaType.AVMEDIA_TYPE_AUDIO)
                {
                    // do something with audio here.
                }
            }
        }
        private int ReadOne()
        {
            Ret = ffmpeg.av_read_frame(Format, Packet);
            if (Ret == ffmpeg.AVERROR_EOF)
            {
                return Update(FfccState.DONE,Ret);
            }
            else if (Ret < 0)
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
            return Update(FfccState.FRAME,Ret);
        }
        public int GetFrame()
        {
            Ret = 0;
            if(Packet->size == 0) return Update(FfccState.READONE);

            Ret = ffmpeg.avcodec_receive_frame(Codec, Frame);
            if (Ret == ffmpeg.AVERROR(ffmpeg.EAGAIN))
                // The decoder doesn't have enough data to produce a frame
                return Update(FfccState.READONE);
            else if (Ret == ffmpeg.AVERROR_EOF)
                // End of file..
                return Update(FfccState.DONE,Ret);
            else CheckRet();
            return Update(FfccState.WAITING,Ret);
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
