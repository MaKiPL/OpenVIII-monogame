using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public sealed class ArchiveWorker : ArchiveBase
    {
        #region Fields

        public readonly ArchiveBase FsArchive;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Saves the active archive and file list.
        /// </summary>
        /// <param name="archive">Memory.Archive</param>
        /// <param name="skipList">If list generation is unneeded you can skip it by setting true</param>
        private ArchiveWorker(Memory.Archive archive, bool skipList = false)
        {
            if (archive.IsDir)
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}:: opening directory: {archive}");
                IsDir = true;
            }
            else
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}:: opening archiveFile: {archive}");
            Archive = archive;
            ParentPath = FindParentPath(archive);
            ArchiveBase tempArchive = null;
            if (ParentPath != null && ParentPath.Count > 0)
                foreach (var p in ParentPath)
                {
                    if (tempArchive != null)
                    {
                        tempArchive = tempArchive.GetArchive(p);
                    }
                    else if (p.IsDir || p.IsFile)
                    {
                        tempArchive = ArchiveBase.Load(p);
                    }
                }
            if (tempArchive != null)
            {
                tempArchive.GetArchive(archive, out var fi, out FsArchive, out var fl);
                ArchiveMap = new ArchiveMap(fi, fl, tempArchive.GetMaxSize(archive));
            }
            if (!skipList)
                GetListOfFiles();
            IsOpen = true;
        }

        private ArchiveWorker(Memory.Archive archive, StreamWithRangeValues fI, ArchiveBase fS, StreamWithRangeValues fL, bool skipList = false)
        {
            ArchiveMap = new ArchiveMap(fI, fL, fS.GetMaxSize(archive));
            Archive = archive;
            FsArchive = fS;
            //FS = null;
            if (!skipList)
                GetListOfFiles();

            IsOpen = true;
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Load Archive with out storing FS in byte[] works for archive that aren't compressed.
        /// </summary>
        /// <param name="path">Archive file archive.</param>
        /// <param name="fI">Stream containing the FI file</param>
        /// <param name="fS">Archive where the FS file is.</param>
        /// <param name="fL">Stream containing the FL file</param>
        /// <param name="skipList">Skip generating list of files</param>
        /// <returns>ArchiveWorker</returns>
        public static ArchiveBase Load(Memory.Archive path, StreamWithRangeValues fI, ArchiveBase fS, StreamWithRangeValues fL, bool skipList = false)
        {
            if (CacheTryGetValue(path, out var value))
            {
                return value;
            }

            value = new ArchiveWorker(path, fI, fS, fL, skipList);
            if (!value.IsOpen)
                value = null;
            if (CacheTryAdd(path, value))
            {
            }
            return value;
        }

        public static ArchiveBase Load(Memory.Archive path, bool skipList = false)
        {
            if (path.IsZZZ)
                return ArchiveZzz.Load(path, skipList);
            if (CacheTryGetValue(path, out var value))
            {
                return value;
            }

            value = new ArchiveWorker(path, skipList);
            if (!value.IsOpen)
                value = null;
            if (CacheTryAdd(path, value))
            {
            }
            return value;
        }

        public override ArchiveBase GetArchive(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileNotFoundException("NO FILENAME");
            if (ArchiveMap != null && ArchiveMap.Count > 1)
                FindFile(ref fileName);
            else if (IsDir)
            {
                if (FileList == null || FileList.Length == 0)
                    ProduceFileLists();
                fileName = FileList.FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            if (File.Exists(Archive.FL))
                using (var fs = new FileStream(Archive.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    FindFile(ref fileName, fs);
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            if (archive == Memory.Archives.ZZZ_MAIN || archive == Memory.Archives.ZZZ_OTHER)
            {
                var zzz = archive.ZZZ;
                if (FindFile(ref zzz) <= -1 || string.IsNullOrWhiteSpace(zzz)) return null;
                if (File.Exists(zzz))
                    archive.SetFilename(zzz);
                return !CacheTryGetValue(archive, out var ab) ? ArchiveZzz.Load(zzz) : ab;
            }

            if (CacheTryGetValue(archive, out var value)) return value;
            GetArchive(archive, out var fI, out var fS, out StreamWithRangeValues fL);
            return fI == null || fS == null || fL == null ||
                   fI.Length == 0 || fL.Length == 0
                ? null
                : new ArchiveWorker(archive, fI, fS, fL);
        }

        /// <summary>
        /// Get binary data
        /// </summary>
        /// <param name="fileName">filename you want</param>
        /// <param name="cache">if true store the data for later</param>
        /// <returns></returns>
        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileNotFoundException("NO FILENAME");
            byte[] FileInTwoArchives()
            {
                //if (FS != null && FS.Length > 0)
                //    return ArchiveMap.GetBinaryFile(filename, new MemoryStream(FS));
                //else
                return FsArchive != null ? ArchiveMap.GetBinaryFile(fileName, FsArchive.GetStreamWithRangeValues(Archive.FS)) : null;
            }
            if (ArchiveMap != null && ArchiveMap.Count > 0)
            {
                if (!cache) return FileInTwoArchives();
                var fileNameBackup = fileName;
                GetListOfFiles();
                fileName = FileList.OrderBy(x => fileName.Length).ThenBy(x => fileNameBackup, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.IndexOf(fileNameBackup, StringComparison.OrdinalIgnoreCase) >= 0);

                if (string.IsNullOrWhiteSpace(fileName)) throw new NullReferenceException($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} fileName ({fileNameBackup}) not found!");

                if (!LocalTryGetValue(fileName, out var value))
                {
                    var buffer = FileInTwoArchives();
                    if (LocalTryAdd(fileName, buffer))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} cached {fileName}");
                    }
                    return buffer;
                }
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)}::{nameof(LocalTryGetValue)} read from cache {fileName}");
                return value;
            }

            if (!IsDir)
            {
                var loc = -1;
                if (File.Exists(Archive.FL))
                    using (var fs = new FileStream(Archive.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        loc = FindFile(ref fileName, fs); //File.OpenRead(archive.FL));

                // read file list

                if (loc == -1)
                {
                    Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: {Archive.FL}");
                    //throw new Exception("ArchiveWorker: No such file!");
                }
                else
                    return GetBinaryFile(fileName, loc, cache);
            }
            else
                return GetBinaryFile(fileName, 0, cache);

            Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: Searched {Archive} and could not find {fileName}");
            return null;
        }

        public override Memory.Archive GetPath() => Archive;

        public override StreamWithRangeValues GetStreamWithRangeValues(string fileName) =>
            GetStreamWithRangeValues(fileName, null, 0);

        public override string ToString() => $"{Archive} :: {Used}";

        protected override int FindFile(ref string filename)
        {
            if (ArchiveMap != null && ArchiveMap.Count > 1)
                return base.FindFile(ref filename);
            if (IsDir)
            {
                var f = filename;
                filename = FileList.FirstOrDefault(x => x.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
                return string.IsNullOrWhiteSpace(filename) ? -1 : 0;
            }

            using (var fs = new FileStream(Archive.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return FindFile(ref filename, fs); //File.OpenRead(archive.FL));
        }

        /// <summary>
        /// Generate a file list from raw text file.
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/12744725/how-do-i-perform-file-readalllines-on-a-file-that-is-also-open-in-excel"/>
        protected override string[] ProduceFileLists()
        {
            string[] r;
            if (Archive == null) return null;
            if (IsDir)
                return Directory.GetFiles(Archive, "*", SearchOption.AllDirectories).OrderBy(x => x.Length)
                    .ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            if ((r = base.ProduceFileLists()) != null)
                return r;
            if (!File.Exists(Archive.FL)) return null;
            using (var fs = new FileStream(Archive.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                return ProduceFileLists(fs).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
        }

        /// <summary>
        /// Search file list for line filename is on.
        /// </summary>
        /// <param name="filename">filename or archive to search for</param>
        /// <param name="stream">stream of text data to search in</param>
        /// <returns>-1 on error or &gt;=0 on success.</returns>
        private static int FindFile(ref string filename, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filename)) return -1;
            filename = filename.TrimEnd('\0');

            using (TextReader tr = new StreamReader(stream))
            {
                for (var i = 0; GetLine(tr, out var line); i++)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Debug.WriteLine("ArchiveWorker::File entry is null. Returning -1");
                        break;
                    }

                    if (line.IndexOf(filename, StringComparison.OrdinalIgnoreCase) < 0) continue;
                    filename = line; //make sure the filename in dictionary is consistent.
                    return i;
                }
            }
            Debug.WriteLine($"ArchiveWorker:: Filename {filename}, not found. returning -1");
            return -1;
        }

        private static bool GetLine(TextReader tr, out string line)
        {
            line = tr.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) return false;
            line = line.TrimEnd('\0');
            return true;
        }

        private static IEnumerable<string> ProduceFileLists(Stream s)
        {
            using (var sr = new StreamReader(s, System.Text.Encoding.UTF8))
            {
                var fl = new List<string>();
                while (!sr.EndOfStream)
                    fl.Add(sr.ReadLine()?.Trim('\0', '\r', '\n'));
                return fl.ToArray();
            }
        }

        private byte[] GetBinaryFile(string fileName, int loc, bool cache)
        {
            fileName = FileList.FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (LocalTryGetValue(fileName, out var b))
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: read from cache: {fileName}");
                return b;
            }
            if (IsDir)
            {
                if (FileList == null || FileList.Length == 0)
                    ProduceFileLists();
                if (!string.IsNullOrWhiteSpace(fileName))
                    using (var br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: reading: {fileName}");
                        var buffer = br.ReadBytes(checked((int)br.BaseStream.Length));
                        if (cache && LocalTryAdd(fileName, buffer))
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: cached: {fileName}");
                        }
                        return buffer;
                    }
            }
            //read index data

            var fi = GetFi(loc);
            //read binary data.
            var temp = GetCompressedData(fi, out var _);

            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: extracting: {fileName}");

            if (temp != null)
                switch (fi.CompressionType)
                {
                    case CompressionType.None:
                        break;

                    case CompressionType.LZSS:
                        LZSS.DecompressAllNew(temp, fi.UncompressedSize);
                        break;

                    case CompressionType.LZ4:
                        temp = ArchiveMap.Lz4Uncompress(temp, fi.UncompressedSize);
                        break;

                    case CompressionType.LZSS_UnknownSize:
                        LZSS.DecompressAllNew(temp, 0);
                        break;

                    case CompressionType.LZSS_LZSS:
                        LZSS.DecompressAllNew(LZSS.DecompressAllNew(temp, fi.UncompressedSize), 0);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            if (temp != null && cache && LocalTryAdd(fileName, temp))
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: cached: {fileName}");
            }
            return temp;
        }

        /// <summary>
        /// GetCompressedData reads data from file directly. This isn't used right now I tried to add checks but they aren't tested yet.
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="size"></param>
        /// <param name="skipData"></param>
        /// <returns></returns>
        private byte[] GetCompressedData(FI fi, out int size, bool skipData = false)
        {
            byte[] temp = null;
            FileStream fs;
            using (var br = new BinaryReader(fs = new FileStream(Archive.FS, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                br.BaseStream.Seek(fi.Offset, SeekOrigin.Begin);
                var compressedSize = 0;
                var max = (int)(fs.Length - fs.Position);
                switch (fi.CompressionType)
                {
                    case CompressionType.None:
                    case CompressionType.LZSS_UnknownSize:
                        compressedSize = fi.UncompressedSize;
                        break;

                    case CompressionType.LZSS:
                    case CompressionType.LZSS_LZSS:
                        if (fs.Length < 5) throw new InvalidDataException();
                        compressedSize = br.ReadInt32();
                        if (compressedSize > (max -= sizeof(int))) throw new InvalidDataException();
                        break;

                    case CompressionType.LZ4:
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                // CompressionType = LZS then Compressed Size is defined. Else the Uncompressed size should work for
                size = compressedSize > 0 ? compressedSize : fi.UncompressedSize > max ? max : fi.UncompressedSize;
                if (size > max)
                {
                    throw new InvalidDataException(
                        $"{nameof(ArchiveWorker)}::{nameof(GetCompressedData)} Expected size ({size}) > ({max})");
                }
                if (!skipData)
                    temp = br.ReadBytes(size);
            }
            return temp;
        }

        private FI GetFi(int loc)
        {
            using (var br = new BinaryReader(new FileStream(Archive.FI, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))//File.OpenRead(archive.FI)))
            {
                br.BaseStream.Seek(loc * 12, SeekOrigin.Begin);
                return Extended.ByteArrayToClass<FI>(br.ReadBytes(12));
            }
        }

        private StreamWithRangeValues GetStreamWithRangeValues(string fileName, FI inputFi, int size)
        {
            void msg() =>
            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetStreamWithRangeValues)} stream: {fileName}");
            if (inputFi != null)
            {
            }

            if (Archive == null) return null;
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileNotFoundException("NO FILENAME");
            if (IsDir)
            {
                FindFile(ref fileName);
                msg();
                var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                if (inputFi == null)
                    return new StreamWithRangeValues(fs, 0, fs.Length);
                return new StreamWithRangeValues(fs, inputFi.Offset, size, inputFi.CompressionType, inputFi.UncompressedSize);
            }

            Debug.Assert(inputFi == null);
            var parentFs = Archive.FS;
            if (ArchiveMap != null && ArchiveMap.Count > 1)
            {
                var fi = ArchiveMap.FindString(ref fileName, out size);
                msg();
                if (FsArchive == null) return null;
                if (fi == null) return FsArchive.GetStreamWithRangeValues(fileName);
                var parentFi = FsArchive.ArchiveMap?.FindString(ref parentFs, out var _);
                return parentFi == null || parentFi.CompressionType == 0 || (FsArchive is ArchiveZzz)
                    ? new StreamWithRangeValues(FsArchive.GetStreamWithRangeValues(parentFs), fi.Offset, size,
                        fi.CompressionType, fi.UncompressedSize)
                    : new StreamWithRangeValues(new MemoryStream(FsArchive.GetBinaryFile(parentFs, true), false),
                        fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
            }

            var loc = -1;
            if (File.Exists(Archive.FL))
                using (var fs = new FileStream(Archive.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    loc = FindFile(ref fileName, fs); //File.OpenRead(archive.FL));

            msg();
            // read file list

            if (loc == -1)
            {
                Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: {Archive.FL}");
                //throw new Exception("ArchiveWorker: No such file!");
            }
            else
            {
                var fi = GetFi(loc);
                GetCompressedData(fi, out size, true);
                return new StreamWithRangeValues(new FileStream(parentFs, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
            }

            Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: Searched {Archive} and could not find {fileName}");
            return null;
        }

        #endregion Methods
    }
}