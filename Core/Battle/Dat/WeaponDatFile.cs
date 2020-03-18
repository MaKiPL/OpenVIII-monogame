namespace OpenVIII.Battle.Dat
{
    public sealed class WeaponDatFile : DatFile
    {
        public static WeaponDatFile CreateInstance(int fileId, int additionalFileId = -1,
            DatFile skeletonReference = null, Sections flags = Sections.All)
        {
            return new WeaponDatFile(fileId, additionalFileId, skeletonReference, flags);
        }

        private WeaponDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Weapon, additionalFileId, skeletonReference, flags)
        {
        }
    }
}