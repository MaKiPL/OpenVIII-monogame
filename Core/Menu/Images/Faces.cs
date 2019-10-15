using System.Collections.Generic;

namespace OpenVIII
{
    public sealed partial class Faces : SP2
    {
        #region Constructors

        /// <summary>
        /// Face images used in menus / save / load screens.
        /// </summary>
        public Faces()
        {
        }

        public static Faces Load() => Load<Faces>();

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps("face{0:0}.tex",2,new BigTexProps("Face_b{0:00}.TEX",2),new BigTexProps("Gf_big{0:00}.TEX",2)),
            };
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            EntriesPerTexture = 16;
        }

        #endregion Constructors
    }
}