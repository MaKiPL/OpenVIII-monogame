using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class ArchiveZZZ : ArchiveBase
    {
        #region Fields

        private Header headerData;

        #endregion Fields

        #region Constructors

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
            IsOpen = true;
        }

        #endregion Constructors

        #region Properties

        public List<Memory.Archive> ParentPath { get; }

        #endregion Properties

        #region Methods
        static object locker = new object(); //prevent two loads at the same time.
        public static ArchiveBase Load(Memory.Archive path, bool skiplist = false)
        {
            lock (locker)
            {
                if (ArchiveBase.TryGetValue(path, out ArchiveBase value))
                {
                    return value;
                }
                else
                {
                    value = new ArchiveZZZ(path, skiplist);
                    if (!value.IsOpen)
                        value = null;
                    if (ArchiveBase.TryAdd(value?.GetPath()?? path, value))
                    {
                    }
                }
                return value;
            }
        }

        public override ArchiveBase GetArchive(string fileName)
        {
            fileName = headerData.GetFilenames().FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            if (!ArchiveBase.TryGetValue(archive, out ArchiveBase value))
                return ArchiveWorker.Load(archive, GetStreamWithRangeValues(archive.FI), this, GetStreamWithRangeValues(archive.FL));
            return value;
        }

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (headerData != null)
            {
                FileData filedata = GetFileData(fileName);
                if (LocalTryGetValue(filedata.Filename, out BufferWithAge value))
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(TryGetValue)} read from cache {filedata.Filename}");
                    return value;
                }
                else
                {
                    BinaryReader br;
                    if (filedata != default && (br = Open()) != null)
                        using (br)
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} extracting {filedata.Filename}");
                            br.BaseStream.Seek(filedata.Offset, SeekOrigin.Begin);
                            byte[] buffer = br.ReadBytes(checked((int)filedata.Size));
                            if (cache && LocalTryAdd(filedata.Filename, buffer))
                            {
                                Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} caching {filedata.Filename}");
                            }
                            return buffer;
                        }
                    else
                        Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} FAILED extracting {fileName}");
                }
            }

            return null;
        }

        public FileData GetFileData(string fileName) => headerData.OrderBy(x => x.Filename.Length).ThenBy(x => x.Filename, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Filename.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);

        public override string[] GetListOfFiles()
        {
            if (FileList == null) FileList = ProduceFileLists();
            return FileList;
        }

        public override Memory.Archive GetPath() => _path;

        public override StreamWithRangeValues GetStreamWithRangeValues(string filename, FI fi = null, int size = 0)
        {
            if (headerData != null)
            {
                FileData filedata = headerData.OrderBy(x => x.Filename.Length).ThenBy(x => x.Filename, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.Filename.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0);
                //if (string.IsNullOrWhiteSpace(fileName)) return null;
                //FileData filedata = headerData.First(x => x.Filename == fileName);

                Stream s;
                if (filedata != default && (s = OpenStream()) != null)
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetStreamWithRangeValues)} got stream of {filename}");
                    if (fi != null)
                        return new StreamWithRangeValues(s, filedata.Offset + fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                    else
                        return new StreamWithRangeValues(s, filedata.Offset, filedata.Size);
                }
                else
                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetStreamWithRangeValues)} FAILED locating {filename}");
            }

            return null;
        }

        public BinaryReader Open()
        {
            Stream s = OpenStream();
            if (s != null)
                return new BinaryReader(s);
            else
                return null;
        }

        public Stream OpenStream()
        {
            string path = File.Exists(_path) && _path.IsZZZ ? (string)_path : File.Exists(_path.ZZZ) ? _path.ZZZ : null;
            if (path != null)
                return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return null;
        }

        public override string ToString() => $"{_path} :: {Used}";

        private string[] ProduceFileLists() => headerData?.GetFilenames().ToArray();

        #endregion Methods
    }
}