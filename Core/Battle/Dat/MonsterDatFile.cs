namespace OpenVIII.Battle.Dat
{
    public sealed class MonsterDatFile : DatFile
    {
        #region Constructors

        private MonsterDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Monster, additionalFileId, skeletonReference,
            flags)
        {
        }

        #endregion Constructors

        #region Methods

        public static MonsterDatFile CreateInstance(int fileId, Sections flags = Sections.All) => new MonsterDatFile(fileId, flags: flags);

        #endregion Methods
    }
}