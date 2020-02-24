using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public partial class ArchiveZZZ : ArchiveBase
    {
        #region Fields

        private ArchiveMap headerData;

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

        private static object locker = new object(); //prevent two loads at the same time.

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
                    if (ArchiveBase.TryAdd(value?.GetPath() ?? path, value))
                    {
                    }
                }
                return value;
            }
        }

        public override ArchiveBase GetArchive(string fileName)
        {
            fileName = GetFileData(fileName).Key;
            if (string.IsNullOrWhiteSpace(fileName)) return null;
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            if (!ArchiveBase.TryGetValue(archive, out ArchiveBase value))
                return ArchiveWorker.Load(archive, GetStreamWithRangeValues(archive.FI), this, GetStreamWithRangeValues(archive.FL));
            return value;
        }

        private static object BinaryFileLock = new object();

        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (headerData != null)
            {
                lock (BinaryFileLock)
                {
                    KeyValuePair<string, FI> filedata = GetFileData(fileName);
                    if (LocalTryGetValue(filedata.Key, out BufferWithAge value))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(TryGetValue)} read from cache {filedata.Key}");
                        return value;
                    }
                    else
                    {
                        BinaryReader br;
                        if (!string.IsNullOrWhiteSpace(filedata.Key) && (br = Open()) != null)
                            using (br)
                            {
                                //Debug.Assert(!fileName.ToLower().Contains("field.fs"));
                                Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} extracting {filedata.Key}");
                                br.BaseStream.Seek(filedata.Value.Offset, SeekOrigin.Begin);
                                byte[] buffer = br.ReadBytes(filedata.Value.UncompressedSize);
                                if (cache && LocalTryAdd(filedata.Key, buffer))
                                {
                                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} caching {filedata.Key}");
                                }
                                return buffer;
                            }
                        else
                            Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetBinaryFile)} FAILED extracting {fileName}");
                    }
                }
            }

            return null;
        }

        public KeyValuePair<string, FI> GetFileData(string fileName) => headerData.GetFileData(fileName);

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
                //Debug.Assert(Path.GetExtension(filename).ToLower() != ".fs");
                KeyValuePair<string, FI> filedata = GetFileData(filename);
                if (string.IsNullOrWhiteSpace(filedata.Key) && (filedata.Value.CompressionType != 0))
                {
                    return new StreamWithRangeValues(new MemoryStream(GetBinaryFile(filedata.Key, true), false), 0, filedata.Value.UncompressedSize);
                }
                //if (string.IsNullOrWhiteSpace(fileName)) return null;
                //FileData filedata = headerData.First(x => x.Filename == fileName);

                Stream s;
                if (!string.IsNullOrWhiteSpace(filedata.Key) && (s = OpenStream()) != null)
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveZZZ)}::{nameof(GetStreamWithRangeValues)} stream: {filename}");
                    if (fi != null)
                        return new StreamWithRangeValues(s, filedata.Value.Offset + fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                    else
                        return new StreamWithRangeValues(s, filedata.Value.Offset, filedata.Value.UncompressedSize);
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

        private string[] ProduceFileLists() => headerData?.OrderedByName.Select(x => x.Key).ToArray();

        #endregion Methods
    }
}