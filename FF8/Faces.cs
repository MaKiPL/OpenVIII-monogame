namespace FF8
{
    internal partial class Faces : SP2
    {

        #region Constructors
        /// <summary>
        /// Face images used in menus / save / load screens.
        /// </summary>
        public Faces()
        {
            TextureBigFilename = new string[]{ "Face_b{0:00}.TEX","Gf_big{0:00}.TEX"};
            TextureBigSplit = new uint[] { 2, 2 };
            TextureFilename = "face{0:0}.tex";
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            TextureCount = 2;
            EntriesPerTexture = 16;
            Init();
        }

#endregion Constructors
#region Enums

        #endregion Enums
    }
}