using FFmpeg.AutoGen;
using System;



namespace FF8
{
    internal class FfccVaribleGroup : IDisposable
    {
        #region Fields
        public unsafe AVFormatContext* _format;
        public unsafe AVPacket* _packet;
        public unsafe AVFrame* _frame;
        public unsafe AVCodecContext* _codecContext;
        public unsafe AVCodec* _codec;
        #endregion
        #region Properties
        /// <summary>
        /// Format holds alot of file info. File is opened and data about it is stored here.
        /// </summary>
        public unsafe AVFormatContext* Format { get => _format; set => _format = value; }
        /// <summary>
        /// Packet of data can contain 1 or more frames.
        /// </summary>
        public unsafe AVPacket* Packet { get => _packet; set => _packet = value; }
        /// <summary>
        /// Frame holds a chunk of data related to the current stream. 
        /// </summary>
        public unsafe AVFrame* Frame { get => _frame; set => _frame = value; }
        /// <summary>
        /// struct that holds info about the related codec for the current stream.
        /// </summary>
        public unsafe AVCodecContext* CodecContext { get => _codecContext; set => _codecContext = value; }
        /// <summary>
        /// The codec of current stream.
        /// </summary>
        public unsafe AVCodec* Codec { get => _codec; set => _codec = value; }
        /// <summary>
        /// pointer to stream
        /// </summary>
        unsafe public AVStream* Stream { get; set; }
        /// <summary>
        /// index of stream
        /// </summary>
        public int StreamIndex { get; set; } = -1;
        /// <summary>
        /// Path and filename of file.
        /// </summary>
        public string FileName { get; set; }

        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        unsafe public FfccVaribleGroup()
        {
            Format = ffmpeg.avformat_alloc_context();
            Packet = ffmpeg.av_packet_alloc();
            Frame = ffmpeg.av_frame_alloc();
            //CodecContext = codecContext;
            //Codec = codec;
            //Stream = stream;
            //StreamIndex = streamIndex;
            //FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }

        ~FfccVaribleGroup()
        {
            Dispose();
        }
        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                ffmpeg.av_packet_unref(Packet);
                ffmpeg.av_free(Packet);

                ffmpeg.av_frame_unref(Frame);
                ffmpeg.av_free(Frame);

                ffmpeg.avcodec_close(CodecContext);
                fixed (AVFormatContext** tmp = &_format)
                {
                    ffmpeg.avformat_close_input(tmp);
                }                

                ffmpeg.avformat_free_context(Format);
                //fixed (AVCodecContext** tmp = &_codecContext)
                //{
                //    ffmpeg.avcodec_free_context(tmp);//throws exception
                //}
                ffmpeg.av_free(Codec);
                //ffmpeg.av_free(Stream); //throws exception

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FfccVaribleGroup() {
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
