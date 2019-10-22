namespace OpenVIII
{
    using FFmpeg.AutoGen;

    public class FfVideo : Ffcc
    {
        #region Methods

        /// <summary>
        /// Opens filename and init class.
        /// </summary>
        public static FfVideo Create(string filename) => Create<FfVideo>(filename, AVMediaType.AVMEDIA_TYPE_VIDEO, FfccMode.STATE_MACH);

        #endregion Methods
    }
}