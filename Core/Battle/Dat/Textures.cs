namespace OpenVIII.Battle.Dat
{
    public struct Textures
    {
        /// <summary>
        /// TIM Count
        /// </summary>
        public uint cTims;

        /// <summary>
        /// File pointers
        /// </summary>
        public uint[] pTims;

        /// <summary>
        /// EOF
        /// </summary>
        public uint Eof;

        /// <summary>
        /// Texture 2D wrapped in TextureHandler for mod support
        /// </summary>
        public TextureHandler[] textures;
    }
}