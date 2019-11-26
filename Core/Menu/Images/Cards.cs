using System.Collections.Generic;

namespace OpenVIII
{
    public sealed partial class Cards : SP2
    {
        #region Constructors

        /// <summary>
        /// Card images used in menus. The images used in the triple triad game are in the ff8.exe in
        /// tim files.
        /// </summary>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=11084.0"/>
        public Cards()
        {
        }

        #endregion Constructors

        #region Methods

        public static Cards Load() => Load<Cards>();

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Props = new List<TexProps>()
            {
                new TexProps("mc{0:00}.tex",10),
            };
            TextureStartOffset = 0;
            EntriesPerTexture = 11;
            IndexFilename = "cardanm.sp2";
        }

        protected override void Init()
        {
            base.Init();
            Entries[(uint)ID.Card_Back] = new Entry
            {
                X = 192,
                Y = 128,
                Width = 64,
                Height = 64
            };
        }

        #endregion Methods
    }
}