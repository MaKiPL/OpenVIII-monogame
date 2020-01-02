namespace OpenVIII.AV
{
    using FFmpeg.AutoGen;

    public class Video : Ffcc
    {
        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static Video Load(string filename) => Load<Video>(filename, AVMediaType.AVMEDIA_TYPE_VIDEO, FfccMode.STATE_MACH);

        #endregion Methods
    }
}