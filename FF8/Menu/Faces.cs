namespace FF8
{
    public partial class Faces : SP2
    {
        #region Constructors

        /// <summary>
        /// Face images used in menus / save / load screens.
        /// </summary>
        public Faces()
        {
            TextureBigFilename = new string[] { "Face_b{0:00}.TEX", "Gf_big{0:00}.TEX" };
            TextureBigSplit = new uint[] { 2, 2 };
            TextureFilename = new string[] { "face{0:0}.tex" };
            TextureCount = new int[] { 2 };
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            EntriesPerTexture = 16;
            Init();
        }

        #endregion Constructors
    }
}