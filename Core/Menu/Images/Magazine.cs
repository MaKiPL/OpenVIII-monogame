using System.Collections.Generic;

namespace OpenVIII
{
    public sealed partial class Magazine : SP2
    {
        #region Constructors

        /// <summary>
        /// Contains Magazines and parts of tutorials; some tutorial images are in Icons.
        /// </summary>
        /// TODO test this.
        public Magazine() { }

        #endregion Constructors

        #region Methods

        public static Magazine Load() => Load<Magazine>();

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps{Filename = "mag{0:00}.tex",Count =20 },
                new TexProps{Filename="magita.TEX",Count =1},
            };
            TextureStartOffset = 0;
            IndexFilename = "";
            EntriesPerTexture = -1;
        }

        /// <summary>
        /// not used in magzine there is no sp2 file.
        /// </summary>
        /// <param name="aw"></param>
        protected override void InitEntries(ArchiveWorker aw = null) { }

        #endregion Methods
    }
}