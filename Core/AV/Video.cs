namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;

    public class Video : Ffcc
    {

        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static Video Load(string filename) =>
            Load<Video>(filename, AVMediaType.AVMEDIA_TYPE_VIDEO, FfccMode.STATE_MACH);

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        /// <remarks>
        /// Could be better, but theres alot of hoops to jump through.
        /// </remarks>
        /// <see cref="https://stackoverflow.com/questions/9604633/reading-a-file-located-in-memory-with-libavformat"/>
        /// <seealso cref="http://www.ffmpeg.org/doxygen/trunk/doc_2examples_2avio_reading_8c-example.html"/>
        /// <seealso cref="https://stackoverflow.com/questions/24758386/intptr-to-callback-function"/>
        public static unsafe Video Load(BufferData buffer_Data, byte[] headerData, int loopstart = -1, FfccMode ffccMode = FfccMode.PROCESS_ALL) =>
            Load(&buffer_Data, headerData, loopstart, ffccMode);

        public static unsafe Video Load(BufferData* buffer_Data, byte[] headerData, int loopstart = -1, FfccMode ffccMode = FfccMode.PROCESS_ALL) =>
            Load<Video>(buffer_Data, headerData, loopstart = -1, ffccMode, AVMediaType.AVMEDIA_TYPE_VIDEO);

        #endregion Methods

    }
}