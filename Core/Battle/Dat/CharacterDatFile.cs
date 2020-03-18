namespace OpenVIII.Battle.Dat
{
    public sealed class CharacterDatFile : DatFile
    {
        public static CharacterDatFile CreateInstance(int fileId, int additionalFileId = -1,
            Sections flags = Sections.All)
        {
            return new CharacterDatFile(fileId, additionalFileId, flags: flags);
        }

        private CharacterDatFile(int fileId, int additionalFileId = -1, DatFile skeletonReference = null,
            Sections flags = Sections.All) : base(fileId, EntityType.Character, additionalFileId, skeletonReference,
            flags)
        {
        }
    }
}