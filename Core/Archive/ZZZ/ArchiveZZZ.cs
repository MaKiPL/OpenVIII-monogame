using System;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class ArchiveZZZ : ArchiveBase
    {
        public override ArchiveBase GetArchive(string fileName)
        {
            fileName = headerData.GetFilenames().FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive) => new ArchiveWorker(GetBinaryFile(archive.FI), GetBinaryFile(archive.FS), GetBinaryFile(archive.FL));

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            fileName = headerData.GetFilenames().FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            FileData filedata = headerData.First(x => x.Filename == fileName);
            using (BinaryReader br = Open())
            {
                br.BaseStream.Seek(filedata.Offset, SeekOrigin.Begin);
                return br.ReadBytes(checked((int)filedata.Size));
            }
        }

        public override string[] GetListOfFiles()
        {
            if (FileList == null) FileList = ProduceFileLists();
            return FileList;
        }

        private string[] ProduceFileLists() => headerData?.GetFilenames().ToArray();

        public override Memory.Archive GetPath() => _path;

        public ArchiveZZZ(Memory.Archive path, bool skiplist = false)
        {
            _path = path;
            using (BinaryReader br = Open())
            {
                headerData = Header.Load(br);
            }
            if (!skiplist)
                FileList = ProduceFileLists();
        }

        private BinaryReader Open()
        {
            FileStream fs;
            return new BinaryReader(fs = new FileStream(_path.ZZZ, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        private Header headerData;
        private Memory.Archive _path;
        private string[] FileList;
    }
}