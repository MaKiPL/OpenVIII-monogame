using FFmpeg.AutoGen;
using System;



namespace OpenVIII
{
    public class FfccVaribleGroup : IDisposable
    {

        #region Fields

        public unsafe AVCodec* _codec;
        public unsafe AVCodecContext* _codecContext;
        public unsafe AVFormatContext* _format;
        public unsafe AVFrame* _frame;
        public unsafe AVPacket* _packet;
        private bool disposedValue = false;

        #endregion Fields

        #region Constructors

        public unsafe FfccVaribleGroup()
        {
            Format = ffmpeg.avformat_alloc_context();
            Packet = ffmpeg.av_packet_alloc();
            Frame = ffmpeg.av_frame_alloc();
            CodecContext = null;
            StreamIndex = -1;
        }

        #endregion Constructors

        #region Destructors

        ~FfccVaribleGroup()
        {
            Dispose();
        }

        #endregion Destructors

        #region Properties

        /// <summary>
        /// Codec 
        /// </summary>
        public unsafe AVCodec* Codec { get => _codec; set => _codec = value; }

        /// <summary>
        /// CodecContext 
        /// </summary>
        public unsafe AVCodecContext* CodecContext { get => _codecContext; set => _codecContext = value; }

        /// <summary>
        /// Format holds alot of file info. File is opened and data about it is stored here.
        /// </summary>
        public unsafe AVFormatContext* Format { get => _format; set => _format = value; }
        /// <summary>
        /// Frame holds a chunk of data related to the current stream. 
        /// </summary>
        public unsafe AVFrame* Frame { get => _frame; set => _frame = value; }

        /// <summary>
        /// Packet of data can contain 1 or more frames.
        /// </summary>
        public unsafe AVPacket* Packet { get => _packet; set => _packet = value; }
        /// <summary>
        /// Current Stream based on index
        /// </summary>
        public unsafe AVStream* Stream => StreamIndex >= 0 && Format != null ? Format->streams[StreamIndex] : null;

        /// <summary>
        /// Set Stream Index typically 0 is video 1 is audio, unless no video then 0 is audio. -1 for no stream of type.
        /// </summary>
        public int StreamIndex { get; set; }
        /// <summary>
        /// Type of current Stream.
        /// </summary>
        public unsafe AVMediaType Type => Stream != null ? Stream->codec->codec_type : AVMediaType.AVMEDIA_TYPE_UNKNOWN;

        #endregion Properties

        #region Methods

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                if (_format != null)
                {
                    fixed (AVFormatContext** tmp = &_format)
                    {
                        for (int i = 0; i < Format->nb_streams; i++)
                        {
                            ffmpeg.avcodec_close(Format->streams[i]->codec);
                        }
                        ffmpeg.avformat_close_input(tmp);
                    }
                }
                if (_codecContext != null)
                {
                    ffmpeg.avcodec_close(CodecContext);
                    fixed (AVCodecContext** tmp = &_codecContext)
                    {
                        ffmpeg.avcodec_free_context(tmp); //ctd
                    }
                }

                //ffmpeg.av_free(Codec); //CTD on linux
                ffmpeg.av_packet_unref(Packet);
                ffmpeg.av_free(Packet);
                ffmpeg.av_frame_unref(Frame);
                ffmpeg.av_free(Frame);
                ffmpeg.av_free(Stream);

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        #endregion Methods
    }
}
