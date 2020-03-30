namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;

    public class Audio : Ffcc
    {
        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static Audio Load(string filename, int loopStart = -1) =>
            Load<Audio>(filename, AVMediaType.AVMEDIA_TYPE_AUDIO, FfccMode.StateMach, loopStart);

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        /// <remarks>
        /// Could be better, but there are many hoops to jump through.
        /// </remarks>
        /// <see cref="https://stackoverflow.com/questions/9604633/reading-a-file-located-in-memory-with-libavformat"/>
        /// <seealso cref="http://www.ffmpeg.org/doxygen/trunk/doc_2examples_2avio_reading_8c-example.html"/>
        /// <seealso cref="https://stackoverflow.com/questions/24758386/intptr-to-callback-function"/>
        ///

        public static Audio Load(Sound.Entry entryData, int loopStart = -1, FfccMode ffccMode = FfccMode.ProcessAll) =>
            Load(entryData, entryData.HeaderData, loopStart, ffccMode);

        public static unsafe Audio Load(BufferData bufferData, byte[] headerData, int loopStart = -1, FfccMode ffccMode = FfccMode.ProcessAll) =>
            Load<Audio>(&bufferData, headerData, loopStart, ffccMode, AVMediaType.AVMEDIA_TYPE_AUDIO);

        public static unsafe Audio Load(BufferData* bufferData, byte[] headerData, int loopStart = -1, FfccMode ffccMode = FfccMode.ProcessAll) =>
            Load<Audio>(bufferData, headerData, loopStart, ffccMode, AVMediaType.AVMEDIA_TYPE_AUDIO);

        #endregion Methods
    }
}