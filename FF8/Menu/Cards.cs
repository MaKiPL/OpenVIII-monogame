namespace FF8
{
    public partial class Cards : SP2
    {
        #region Constructors

        /// <summary>
        /// Card images used in menus. The images used in the triple triad game are in the ff8.exe in
        /// tim files.
        /// </summary>
        /// <seealso cref="http://forums.qhimm.com/index.php?topic=11084.0"/>
        public Cards()
        {
            TextureCount[0] = 10;
            TextureFilename[0] = "mc{0:00}.tex";
            TextureStartOffset = 0;
            EntriesPerTexture = 11;
            IndexFilename = "cardanm.sp2";
            Init();
            Entries[(uint)ID.Card_Back] = new Entry
            {
                X = 192,
                Y = 128,
                Width = 64,
                Height = 64
            };
        }

        #endregion Constructors
    }
}