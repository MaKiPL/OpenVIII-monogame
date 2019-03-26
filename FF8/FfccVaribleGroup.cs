using FFmpeg.AutoGen;
using System;



namespace FF8
{
    internal class FfccVaribleGroup : IDisposable
    {
        #region Fields
        public AVFormatContext _format;
        #endregion
        #region Properties
        /// <summary>
        /// Format holds alot of file info. File is opened and data about it is stored here.
        /// </summary>
        public AVFormatContext Format { get => _format; set => _format = value; }
        #endregion
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        ~FfccVaribleGroup()
        {
            Dispose();
        }
        unsafe protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }
                fixed (AVFormatContext* tmp = &_format)
                {
                    ffmpeg.avformat_close_input(&tmp);
                }
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
