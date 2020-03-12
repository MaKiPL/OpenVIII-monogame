namespace OpenVIII.Battle.Dat
{
    public struct Textures
    {
        #region Fields

        /// <summary>
        /// TIM Count
        /// </summary>
        public uint cTims;

        /// <summary>
        /// EOF
        /// </summary>
        public uint Eof;

        /// <summary>
        /// File pointers
        /// </summary>
        public uint[] pTims;

        /// <summary>
        /// Texture 2D wrapped in TextureHandler for mod support
        /// </summary>
        public TextureHandler[] textures;

        #endregion Fields
    }
}