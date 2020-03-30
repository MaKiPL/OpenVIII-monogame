namespace OpenVIII.Battle.Dat
{
    public sealed class CharacterDatFile : DatFile
    {
        #region Constructors

        private CharacterDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Character, additionalFileId, skeletonReference,
            flags)
        {
        }

        #endregion Constructors

        #region Methods

        public static CharacterDatFile CreateInstance(int fileId, int additionalFileId = -1,
                    Sections flags = Sections.All) => new CharacterDatFile(fileId, additionalFileId, flags: flags);

        #endregion Methods
    }
}