namespace OpenVIII
{
    using FFmpeg.AutoGen;

    public class FfAudio : Ffcc
    {
        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static FfAudio Create(string filename, int loopstart = -1) => Create<FfAudio>(filename, AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode.STATE_MACH, loopstart);

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
        public static unsafe FfAudio Play(Buffer_Data buffer_Data, byte[] headerData, string datafilename, int loopstart = -1)
        {
            FfAudio r = new FfAudio();

            fixed (byte* tmp = &headerData[0])
            {
                lock (r.Decoder)
                {
                    buffer_Data.SetHeader(tmp);
                    DataFileName = datafilename;
                    r.LoadFromRAM(&buffer_Data);
                    r.Init(null, AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode.PROCESS_ALL, loopstart);
                    ffmpeg.avformat_free_context(r.Decoder.Format);
                    //ffmpeg.avio_context_free(&Decoder._format->pb); //CTD
                    r.Decoder.Format = null;
                }
                r.Dispose(false);
            }
            return r;
        }

        #endregion Methods
    }
}