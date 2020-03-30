namespace OpenVIII.Battle.Dat
{
    public sealed class WeaponDatFile : DatFile
    {
        #region Constructors

        private WeaponDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Weapon, additionalFileId, skeletonReference, flags)
        {
        }

        #endregion Constructors

        #region Methods

        public static WeaponDatFile CreateInstance(int fileId, int additionalFileId = -1,
                    DatFile skeletonReference = null, Sections flags = Sections.All) => new WeaponDatFile(fileId, additionalFileId, skeletonReference, flags);

        #endregion Methods
    }
}