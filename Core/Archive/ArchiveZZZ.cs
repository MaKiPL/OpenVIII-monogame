using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class ArchiveZZZ : ArchiveBase
    {
        public override ArchiveBase GetArchive(string fileName)
        {
            fileName = headerData.GetFilenames().FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return GetArchive((Memory.Archive)fileName);
        }
        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            return new ArchiveWorker(GetBinaryFile(archive.FI), GetBinaryFile(archive.FS), GetBinaryFile(archive.FL));
        }

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            fileName = headerData.GetFilenames().FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            FileData filedata = headerData.First(x => x.Filename == fileName);
            using (BinaryReader br = new BinaryReader(new FileStream(archive.ZZZ, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                br.BaseStream.Seek(filedata.Offset, SeekOrigin.Begin);
                return br.ReadBytes(checked((int)filedata.Size));
            }
        }

        //public Stream GetBinaryFileStream(string fileName, bool cache = false)
        //{
        //}

        public override string[] GetListOfFiles() => headerData?.GetFilenames().ToArray();

        public override Memory.Archive GetPath() => archive;

        private Memory.Archive archive;
        private Header headerData;

        private struct FileData
        {
            public string Filename { get; set; }
            public long Offset { get; set; }
            public uint Size { get; set; }

            /// <summary>
            /// Decode/Encode the filename string as bytes.
            /// </summary>
            /// <remarks>
            /// Could be Ascii or UTF8, I see no special characters and the first like 127 of UTF8 is the
            /// same as Ascii.
            /// </remarks>
            private static string ConvertFilename(byte[] filenamebytes) => System.Text.Encoding.UTF8.GetString(filenamebytes);

            public static FileData Load(BinaryReader br)
            {
                int FilenameLength = br.ReadInt32();
                byte[] Filenamebytes = br.ReadBytes(FilenameLength);
                return new FileData
                {
                    Offset = br.ReadInt64(),
                    Size = br.ReadUInt32(),
                    Filename = ConvertFilename(Filenamebytes)
                };
            }
        }

        private class Header : IList<FileData>, IReadOnlyList<FileData>
        {
            private Header(int count) => Data = new List<FileData>(count);

            private List<FileData> Data;

            public FileData this[int index] { get => ((IList<FileData>)Data)[index]; set => ((IList<FileData>)Data)[index] = value; }

            public bool IsReadOnly => ((IList<FileData>)Data).IsReadOnly;

            int ICollection<FileData>.Count => ((IList<FileData>)Data).Count;

            public int Count => ((IReadOnlyList<FileData>)Data).Count;

            public void Add(FileData item) => ((IList<FileData>)Data).Add(item);

            public void Clear() => ((IList<FileData>)Data).Clear();

            public bool Contains(FileData item) => ((IList<FileData>)Data).Contains(item);

            public void CopyTo(FileData[] array, int arrayIndex) => ((IList<FileData>)Data).CopyTo(array, arrayIndex);

            public IEnumerator<FileData> GetEnumerator() => ((IList<FileData>)Data).GetEnumerator();

            public int IndexOf(FileData item) => ((IList<FileData>)Data).IndexOf(item);

            public void Insert(int index, FileData item) => ((IList<FileData>)Data).Insert(index, item);

            public bool Remove(FileData item) => ((IList<FileData>)Data).Remove(item);

            public void RemoveAt(int index) => ((IList<FileData>)Data).RemoveAt(index);

            IEnumerator IEnumerable.GetEnumerator() => ((IList<FileData>)Data).GetEnumerator();

            public static Header Load(BinaryReader br)
            {
                Header r = new Header(br.ReadInt32());
                for (int i = 0; i < r.Data.Capacity; i++)
                    r.Data.Add(FileData.Load(br));
                return r;
            }

            public IOrderedEnumerable<string> GetFilenames()
                => Data.Select(x => x.Filename).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase);

            private Header() => Data = new List<FileData>();
        }
    }
}