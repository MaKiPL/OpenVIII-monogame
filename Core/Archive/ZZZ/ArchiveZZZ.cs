using System;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class ArchiveZZZ : ArchiveBase
    {
        #region Fields

        private static object BinaryFileLock = new object();

        private static object getArchiveLock = new object();
        private static object locker = new object();

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
                ArchiveMap = Header.Load(br);
            }
            if (!skiplist)
                GetListOfFiles();
            IsOpen = true;
        }

        #endregion Constructors

        //prevent two loads at the same time.

        #region Methods

        public static ArchiveBase Load(Memory.Archive path, bool skiplist = false)
        {
            lock (locker)
            {
                if (ArchiveBase.CacheTryGetValue(path, out ArchiveBase value))
                {
                    return value;
                }
                else
                {
                    value = new ArchiveZZZ(path, skiplist);
                    if (!value.IsOpen)
                        value = null;
                    if (ArchiveBase.TryAdd(value?.GetPath() ?? path, value))
                    {
                    }
                }
                return value;
            }
        }

        public override ArchiveBase GetArchive(string fileName)
        {
            fileName = ArchiveMap.GetFileData(fileName).Key;
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            lock (getArchiveLock)
            {
                if (!ArchiveBase.CacheTryGetValue(archive, out ArchiveBase value))
                    return ArchiveWorker.Load(archive, GetStreamWithRangeValues(archive.FI), this, GetStreamWithRangeValues(archive.FL));
                return value;
            }
        }

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (ArchiveMap != null)
            {
                lock (BinaryFileLock)
                {
                    FI filedata = ArchiveMap.FindString(ref fileName, out int size);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        if (false && LocalTryGetValue(fileName, out BufferWithAge value))
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(CacheTryGetValue)} read from cache {fileName}");
                            return value;
                        }
                        else
                        {
                            Stream s;
                            if ((s = OpenStream()) != null)
                            {
                                byte[] buffer = ArchiveMap.GetBinaryFile(filedata, s, fileName, size);
                                if (cache && LocalTryAdd(fileName, buffer))
                                {
                                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} caching {fileName}");
                                }
                                return buffer;
                            }
                        }
                    }
                }
            }
            Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} FAILED extracting {fileName}");
            return null;
        }

        public override Memory.Archive GetPath() => _path;

        public override StreamWithRangeValues GetStreamWithRangeValues(string filename, FI fi = null, int size = 0)
        {
            if (ArchiveMap != null)
            {
                //Debug.Assert(Path.GetExtension(filename).ToLower() != ".fs");
                FI filedata = ArchiveMap.FindString(ref filename, out size);
                if (!string.IsNullOrWhiteSpace(filename) && filedata != null)
                {
                    return new StreamWithRangeValues(OpenStream(), filedata.Offset, size, filedata.CompressionType, filedata.UncompressedSize);
                }
                //if (string.IsNullOrWhiteSpace(fileName)) return null;
                //FileData filedata = headerData.First(x => x.Filename == fileName);

                //Stream s;
                //if (!string.IsNullOrWhiteSpace(filedata.Key) && (s = OpenStream()) != null)
                //{
                //    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetStreamWithRangeValues)} stream: {filename}");
                //    if (fi != null)
                //        return new StreamWithRangeValues(s, filedata.Value.Offset + fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                //    else
                //        return new StreamWithRangeValues(s, filedata.Value.Offset, filedata.Value.UncompressedSize);
                //}
                //else
                //    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetStreamWithRangeValues)} FAILED locating {filename}");
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

        #endregion Methods
    }
}