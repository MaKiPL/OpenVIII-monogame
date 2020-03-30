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
            Load<Video>(filename, AVMediaType.AVMEDIA_TYPE_VIDEO);

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        /// <remarks>
        /// Could be better, but there are many hoops to jump through.
        /// </remarks>
        /// <see cref="https://stackoverflow.com/questions/9604633/reading-a-file-located-in-memory-with-libavformat"/>
        /// <seealso cref="http://www.ffmpeg.org/doxygen/trunk/doc_2examples_2avio_reading_8c-example.html"/>
        /// <seealso cref="https://stackoverflow.com/questions/24758386/intptr-to-callback-function"/>
        public static unsafe Video Load(BufferData bufferData, byte[] headerData, FfccMode ffccMode = FfccMode.ProcessAll) =>
            Load<Video>(&bufferData, headerData, -1, ffccMode, AVMediaType.AVMEDIA_TYPE_VIDEO);
        public static unsafe Video Load(BufferData* bufferData, byte[] headerData, FfccMode ffccMode = FfccMode.ProcessAll) =>
            Load<Video>(bufferData, headerData, -1, ffccMode, AVMediaType.AVMEDIA_TYPE_VIDEO);

        #endregion Methods
    }
}