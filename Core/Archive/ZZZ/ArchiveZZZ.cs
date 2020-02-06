using System;
using System.Collections.Generic;
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

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            if (!ArchiveBase.TryGetValue(archive, out ArchiveBase value))                
                return ArchiveWorker.Load(archive, GetBinaryFile(fileName: archive.FI), GetBinaryFile(archive.FS), GetBinaryFile(archive.FL));
            return value;
        }

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (headerData != null)
            {
                FileData filedata = headerData.OrderBy(x => x.Filename.Length).ThenBy(x => x.Filename, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Filename.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
                //if (string.IsNullOrWhiteSpace(fileName)) return null;
                //FileData filedata = headerData.First(x => x.Filename == fileName);
                BinaryReader br;
                if (filedata != default && (br = Open()) != null)
                    using (br)
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} extracting {filedata.Filename}");
                        br.BaseStream.Seek(filedata.Offset, SeekOrigin.Begin);
                        return br.ReadBytes(checked((int)filedata.Size));
                    }
                else
                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} FAILED extracting {fileName}");
            }

            return null;
        }

        public override string[] GetListOfFiles()
        {
            if (FileList == null) FileList = ProduceFileLists();
            return FileList;
        }

        private string[] ProduceFileLists() => headerData?.GetFilenames().ToArray();

        public override Memory.Archive GetPath() => _path;

        public static ArchiveBase Load(Memory.Archive path, bool skiplist = false)
        {
            if (ArchiveBase.TryGetValue(path, out ArchiveBase value))
            {
                return value;
            }
            else if (ArchiveBase.TryAdd(path, value = new ArchiveZZZ(path, skiplist)))
            {
            }
            return value;
        }
        protected ArchiveZZZ(Memory.Archive path, bool skiplist = false)
        {
            ArchiveBase tempArchive = null;
            ParentPath = FindParentPath(path);
            if (ParentPath != null && ParentPath.Count > 0)
                foreach (Memory.Archive p in ParentPath)
                {
                    if (p.IsDir)
                    {
                        tempArchive = ArchiveBase.Load(p);
                    }
                    else if (tempArchive != null)
                    {
                        throw new Exception("zzz shouldn't be inside an archive.");
                    }
                }
            _path = path;
            if (tempArchive != null)
            {
                _path = tempArchive.GetListOfFiles().FirstOrDefault(x => x.IndexOf(path.ZZZ, StringComparison.OrdinalIgnoreCase) > 0);
            }
            Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}:: opening archiveFile: {_path}");
            using (BinaryReader br = Open())
            {
                if (br == null) return;
                headerData = Header.Load(br);
            }
            if (!skiplist)
                FileList = ProduceFileLists();

        }

        private BinaryReader Open()
        {
            FileStream fs;
            string path = File.Exists(_path) && _path.IsZZZ ? (string)_path : File.Exists(_path.ZZZ) ? _path.ZZZ : null;
            if (path != null)
                return new BinaryReader(fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            else
                return null;
        }

        private Header headerData;
        private Memory.Archive _path;
        private string[] FileList;

        public List<Memory.Archive> ParentPath { get; }
    }
}