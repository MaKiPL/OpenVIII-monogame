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
        #endregion
        #region Properties
        /// <summary>
        /// Format holds alot of file info. File is opened and data about it is stored here.
        /// </summary>
        unsafe public AVFormatContext* Format { get => _format; set => _format = value; }
        /// <summary>
        /// Packet of data can contain 1 or more frames.
        /// </summary>
        public unsafe AVPacket* Packet { get => _packet; set => _packet = value; }
        /// <summary>
        /// Frame holds a chunk of data related to the current stream. 
        /// </summary>
        public unsafe AVFrame* Frame { get => _frame; set => _frame = value; }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
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
                fixed (AVFormatContext** tmp = &_format)
                {
                    ffmpeg.avformat_close_input(tmp);
                }

                ffmpeg.av_packet_unref(Packet);
                ffmpeg.av_free(Packet);
                ffmpeg.av_frame_unref(Frame);
                ffmpeg.av_free(Frame);

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
