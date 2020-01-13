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

        #endregion Constructors

        #region Methods

        public static Faces Load() => Load<Faces>();

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps{Filename = "face{0:0}.tex",Count = 2,Big = new List<BigTexProps>{ new BigTexProps{Filename="Face_b{0:00}.TEX",Split =2 },new BigTexProps{Filename="Gf_big{0:00}.TEX",Split = 2 } } },
            };
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            EntriesPerTexture = 16;
        }

        #endregion Methods
    }
}