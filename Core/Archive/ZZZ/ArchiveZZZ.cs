using System;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public sealed partial class ArchiveZzz : ArchiveBase
    {
        #region Fields

        private static readonly object BinaryFileLock = new object();

        private static readonly object GetArchiveLock = new object();
        private static readonly object Locker = new object();

        #endregion Fields

        #region Constructors

        private ArchiveZzz(Memory.Archive archive, bool skipList = false)
        {
            ArchiveBase tempArchive = null;
            ParentPath = FindParentPath(archive);
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
            Archive = archive;
            if (tempArchive != null)
                archive.SetFilename(tempArchive.GetListOfFiles()
                    .FirstOrDefault(x => x.IndexOf(archive.ZZZ, StringComparison.OrdinalIgnoreCase) > 0));
            Memory.Log.WriteLine($"{nameof(ArchiveZzz)}:: opening archiveFile: {archive}");
            using (BinaryReader br = Open())
            {
                if (br == null) return;
                ArchiveMap = Header.Load(br);
            }
            if (!skipList)
                GetListOfFiles();
            IsOpen = true;
        }

        #endregion Constructors

        //prevent two loads at the same time.

        #region Methods

        public static ArchiveBase Load(Memory.Archive path, bool skipList = false)
        {
            lock (Locker)
            {
                if (CacheTryGetValue(path, out ArchiveBase value))
                {
                    return value;
                }
                else
                {
                    value = new ArchiveZzz(path, skipList);
                    if (!value.IsOpen)
                        value = null;
                    if (value == null) return null;
                    if (CacheTryAdd(value.GetPath() ?? path, value))
                    {
                    }
                    path.SetFilename(value.GetPath() ?? path);
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
            lock (GetArchiveLock)
            {
                if (!CacheTryGetValue(archive, out ArchiveBase value))
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
                    FI fi = ArchiveMap.FindString(ref fileName, out int size);
                    if (!string.IsNullOrWhiteSpace(fileName))
                    {
                        if (LocalTryGetValue(fileName, out BufferWithAge value))
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveZzz)}::{nameof(GetBinaryFile)}::{nameof(CacheTryGetValue)} read from cache {fileName}");
                            return value;
                        }
                        else
                        {
                            Stream s;
                            if ((s = OpenStream()) != null)
                            {
                                byte[] buffer = ArchiveMap.GetBinaryFile(fi, s, fileName, size);
                                if (buffer != null && cache && LocalTryAdd(fileName, buffer))
                                {
                                    Memory.Log.WriteLine($"{nameof(ArchiveZzz)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} caching {fileName}");
                                }

                                return buffer;
                            }
                        }
                    }
                }
            }
            Memory.Log.WriteLine($"{nameof(ArchiveZzz)}::{nameof(GetBinaryFile)} FAILED extracting {fileName}");
            return null;
        }

        public override Memory.Archive GetPath() => Archive;

        /// <summary>
        ///
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="fi">remove me?</param>
        /// <param name="size">remove me?</param>
        /// <returns></returns>
        // ReSharper disable once RedundantAssignment
        public override StreamWithRangeValues GetStreamWithRangeValues(string filename)
        {
            if (ArchiveMap == null) return null;
            FI fi = ArchiveMap.FindString(ref filename, out int size);
            return !string.IsNullOrWhiteSpace(filename) && fi != null
                ? new StreamWithRangeValues(OpenStream(), fi.Offset, size, fi.CompressionType, fi.UncompressedSize)
                : null;
        }

        public BinaryReader Open()
        {
            Stream s = OpenStream();
            return s != null ? new BinaryReader(s) : null;
        }

        public Stream OpenStream()
        {
            string path = File.Exists(Archive) && Archive.IsZZZ ? (string)Archive : File.Exists(Archive.ZZZ) ? Archive.ZZZ : null;
            return path != null ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite) : null;
        }

        public override string ToString() => $"{Archive} :: {Used}";

        #endregion Methods
    }
}