using System.IO;

namespace OpenVIII
{
    public partial class ArchiveZZZ
    {
        #region Classes

        private static class Header
        {
            #region Methods

            public static ArchiveMap Load(BinaryReader br)
            {
                int capacity = br.ReadInt32();
                ArchiveMap r = new ArchiveMap(capacity);
                for (int i = 0; i < capacity; i++)
                    r.Add(FileData.Load(br));
                return r;
            }

            #endregion Methods

            //public IOrderedEnumerable<string> GetFilenames()
            //    => Data.Select(x => x.Filename).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase);

            //private Header() => Data = new List<FileData>();
        }

        #endregion Classes
    }
}