using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class ArchiveZZZ
    {
        private static class Header
        {
            public static ArchiveMap Load(BinaryReader br)
            {
                int capacity = br.ReadInt32();
                ArchiveMap r = new ArchiveMap(capacity);
                for (int i = 0; i < capacity; i++)
                    r.Add(FileData.Load(br));
                return r;
            }

            //public IOrderedEnumerable<string> GetFilenames()
            //    => Data.Select(x => x.Filename).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase);

            //private Header() => Data = new List<FileData>();
        }
    }
}