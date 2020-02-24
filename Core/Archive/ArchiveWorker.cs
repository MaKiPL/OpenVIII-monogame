using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class ArchiveWorker : ArchiveBase, IReadOnlyDictionary<string, byte[]>, IEnumerator<KeyValuePair<string, byte[]>>
    {
        #region Fields

        public readonly ArchiveBase FSArchive;

        /// <summary>
        /// prevent two threads from writing to cache at the same time.
        /// </summary>
        private static object cachelock = new object();

        private IEnumerator enumerator;
        //private byte[] FS;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Saves the active archive and file list.
        /// </summary>
        /// <param name="path">Memory.Archive</param>
        /// <param name="skiplist">If list generation is unneeded you can skip it by setting true</param>
        protected ArchiveWorker(Memory.Archive path, bool skiplist = false)
        {
            if (path.IsDir)
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}:: opening directory: {path}");
                isDir = true;
            }
            else
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}:: opening archiveFile: {path}");
            _path = path;
            ParentPath = FindParentPath(path);
            ArchiveBase tempArchive = null;
            if (ParentPath != null && ParentPath.Count > 0)
                foreach (Memory.Archive p in ParentPath)
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
                tempArchive.GetArchive(path, out StreamWithRangeValues FI, out FSArchive, out StreamWithRangeValues FL);
                ArchiveMap = new ArchiveMap(FI, FL);
            }
            if (!skiplist)
                GetListOfFiles();
            IsOpen = true;
        }

        protected ArchiveWorker(Memory.Archive path, StreamWithRangeValues fI, ArchiveBase fS, StreamWithRangeValues fL, bool skiplist = false)
        {
            ArchiveMap = new ArchiveMap(fI, fL);
            _path = path;
            FSArchive = fS;
            //FS = null;
            if (!skiplist)
                GetListOfFiles();

            IsOpen = true;
        }

        //protected ArchiveWorker(Memory.Archive path, byte[] fI, byte[] fS, byte[] fL, bool skiplist = false)
        //{
        //    ArchiveMap = new ArchiveMap(fI, fL);
        //    _path = path;
        //    FS = fS;
        //    if (!skiplist)
        //        GetListOfFiles();

        //    IsOpen = true;
        //}

        #endregion Constructors

        #region Properties

        public int Count => GetListOfFiles().Count();

        public KeyValuePair<string, byte[]> Current => GetCurrent();

        object IEnumerator.Current => GetCurrent();

        public IEnumerable<string> Keys => GetListOfFiles();

        public List<Memory.Archive> ParentPath { get; }

        public IEnumerable<byte[]> Values => GetListOfFiles().Select(x => GetBinaryFile(x));

        internal ArchiveMap ArchiveMap { get; }

        #endregion Properties

        #region Indexers

        public byte[] this[string key]
        {
            get
            {
                if (TryGetValue(key, out byte[] value))
                    return value;
                return null;
            }
        }

        #endregion Indexers

        #region Methods

        /// <summary>
        /// Load Archive with out storing FS in byte[] works for archive that aren't compressed.
        /// </summary>
        /// <param name="path">Archive file path.</param>
        /// <param name="fI">Stream containing the FI file</param>
        /// <param name="fS">Archive where the FS file is.</param>
        /// <param name="fL">Stream containing the FL file</param>
        /// <param name="skiplist">Skip generating list of files</param>
        /// <returns>ArchiveWorker</returns>
        public static ArchiveBase Load(Memory.Archive path, StreamWithRangeValues fI, ArchiveBase fS, StreamWithRangeValues fL, bool skiplist = false)
        {
            if (ArchiveBase.TryGetValue(path, out ArchiveBase value))
            {
                return value;
            }
            else
            {
                value = new ArchiveWorker(path, fI, fS, fL, skiplist);
                if (!value.IsOpen)
                    value = null;
                if (ArchiveBase.TryAdd(path, value))
                {
                }
            }
            return value;
        }

        public static ArchiveBase Load(Memory.Archive path, bool skiplist = false)
        {
            if (ArchiveBase.TryGetValue(path, out ArchiveBase value))
            {
                return value;
            }
            else
            {
                value = new ArchiveWorker(path, skiplist);
                if (!value.IsOpen)
                    value = null;
                if (ArchiveBase.TryAdd(path, value))
                {
                }
            }
            return value;
        }

        ///// <summary>
        ///// Generate archive worker and get file
        ///// </summary>
        ///// <param name="archive">which archive you want to read from</param>
        ///// <param name="fileName">name of file you want to recieve</param>
        ///// <returns>Uncompressed binary file data</returns>
        ////public static byte[] GetBinaryFile(Memory.Archive archive, string fileName, bool cache = false)
        ////{
        ////    ArchiveWorker tmp = new ArchiveWorker(archive, true);
        ////    return tmp.GetBinaryFile(fileName, cache);
        ////}
        //public static ArchiveBase Load(Memory.Archive path, byte[] fI, byte[] fS, byte[] fL, bool skiplist = false)
        //{
        //    if (ArchiveBase.TryGetValue(path, out ArchiveBase value))
        //    {
        //        return value;
        //    }
        //    else
        //    {
        //        value = new ArchiveWorker(path, fI, fS, fL, skiplist);
        //        if (!value.IsOpen)
        //            value = null;
        //        if (ArchiveBase.TryAdd(path, value))
        //        {
        //        }
        //    }
        //    return value;
        //}

        //public void CacheFS()
        //{
        //    if (FSArchive != null)
        //        FS = FSArchive.GetBinaryFile(_path.FS);
        //}

        //public void ClearFS()
        //{
        //    if (FSArchive != null)
        //        FS = null;
        //}

        public bool ContainsKey(string key) => FindFile(ref key) > -1;

        public void Dispose()
        { }

        public override ArchiveBase GetArchive(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileNotFoundException("NO FILENAME");
            if (ArchiveMap != null && ArchiveMap.Count > 1)
                FindFile(ref fileName);
            else if (isDir)
            {
                if (FileList == null || FileList.Length == 0)
                    ProduceFileLists();
                fileName = FileList.FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else
            if (File.Exists(_path.FL))
                using (FileStream fs = new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    FindFile(ref fileName, fs);
            return GetArchive((Memory.Archive)fileName);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive)
        {
            if (archive == Memory.Archives.ZZZ_MAIN || archive == Memory.Archives.ZZZ_OTHER)
            {
                string zzz = archive.ZZZ;
                if (FindFile(ref zzz) > -1 && !string.IsNullOrWhiteSpace(zzz))
                {
                    if (File.Exists(zzz))
                        archive.SetFilename(zzz);
                    if (!ArchiveBase.TryGetValue(archive, out ArchiveBase ab))
                    {
                        return ArchiveZZZ.Load(zzz);
                    }
                    return ab;
                }
            }
            else
            {
                if (!ArchiveBase.TryGetValue(archive, out ArchiveBase value))
                {
                    GetArchive(archive, out StreamWithRangeValues fI, out ArchiveBase fS, out StreamWithRangeValues fL);
                    if (fI == null || fS == null || fL == null ||
                        fI.Length == 0 || fL.Length == 0)
                        return null;
                    return new ArchiveWorker(archive, fI, fS, fL);

                }
                return value;
            }
            return null;
        }

        /// <summary>
        /// GetBinary
        /// </summary>
        /// <param name="fileName"></param>
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
                if (FSArchive != null)
                    return ArchiveMap.GetBinaryFile(fileName, FSArchive.GetStreamWithRangeValues(_path.FS));
                return null;
            }
            if (ArchiveMap != null && ArchiveMap.Count > 0)
            {
                if (cache)
                {
                    string oldfn = fileName;
                    string[] v = GetListOfFiles();
                    fileName = v.OrderBy(x => fileName.Length).ThenBy(x => oldfn, StringComparer.OrdinalIgnoreCase).FirstOrDefault(x => x.IndexOf(oldfn, StringComparison.OrdinalIgnoreCase) >= 0);
                    if (string.IsNullOrWhiteSpace(fileName))
                    {

                    }
                    Debug.Assert(!string.IsNullOrWhiteSpace(fileName));
                    if (!LocalTryGetValue(fileName, out BufferWithAge value))
                    {
                        byte[] buffer = FileInTwoArchives();
                        if (LocalTryAdd(fileName, buffer))
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)}::{nameof(LocalTryAdd)} cached {fileName}");
                        }
                        return buffer;
                    }
                    Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)}::{nameof(LocalTryGetValue)} read from cache {fileName}");
                    return value;
                }
                else
                    return FileInTwoArchives();
            }
            else if (!isDir)
            {
                int loc = -1;
                if (File.Exists(_path.FL))
                    using (FileStream fs = new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        loc = FindFile(ref fileName, fs); //File.OpenRead(_path.FL));

                // read file list

                if (loc == -1)
                {
                    Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: {_path.FL}");
                    //throw new Exception("ArchiveWorker: No such file!");
                }
                else
                    return GetBinaryFile(fileName, loc, cache);
            }
            else
                return GetBinaryFile(fileName, 0, cache);

            Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: Searched {_path} and could not find {fileName}");
            return null;
        }

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        /// <summary>
        /// Get current file list for loaded archive.
        /// </summary>
        public override string[] GetListOfFiles()
        {
            if (FileList == null)
            {
                FileList = ProduceFileLists();
                enumerator = FileList?.GetEnumerator();
            }
            return FileList;
        }

        public override Memory.Archive GetPath() => _path;

        public override StreamWithRangeValues GetStreamWithRangeValues(string fileName, FI inputFI = null, int size = 0)
        {
            void msg() =>
            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetStreamWithRangeValues)} getting {fileName}");
            if (inputFI != null)
            {
            }
            if (_path != null)
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    throw new FileNotFoundException("NO FILENAME");
                else if (isDir)
                {
                    FindFile(ref fileName);
                    msg();
                    FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                    if (inputFI == null)
                        return new StreamWithRangeValues(fs, 0, fs.Length);
                    else
                        return new StreamWithRangeValues(fs, inputFI.Offset, size, inputFI.CompressionType, inputFI.UncompressedSize);
                }
                else
                {
                    Debug.Assert(inputFI == null);
                    if (ArchiveMap != null && ArchiveMap.Count > 1)
                    {
                        FI fi = ArchiveMap.FindString(ref fileName, out size);
                        msg();
                        //if (FS != null && FS.Length > 0)
                        //{
                        //    return new StreamWithRangeValues(new MemoryStream(FS), fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                        //}
                        //else 
                        if (FSArchive != null)
                        {

                            if (fi != null) // unsure about this part.
                            {
                                if (fi.CompressionType != 0 && !(FSArchive is ArchiveZZZ))
                                    return new StreamWithRangeValues(new MemoryStream(FSArchive.GetBinaryFile(_path.FS, true), false), fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                                else
                                    return new StreamWithRangeValues(FSArchive.GetStreamWithRangeValues(_path.FS), fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                            }
                            else
                                return FSArchive.GetStreamWithRangeValues(fileName);
                        }
                        //return GetStreamWithRangeValues(_path.FS, fi, size);
                        else
                            return null;
                    }

                    int loc = -1;
                    if (File.Exists(_path.FL))
                        using (FileStream fs = new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            loc = FindFile(ref fileName, fs); //File.OpenRead(_path.FL));

                    msg();
                    // read file list

                    if (loc == -1)
                    {
                        Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: {_path.FL}");
                        //throw new Exception("ArchiveWorker: No such file!");
                    }
                    else
                    {
                        FI fi = GetFI(loc);
                        GetCompressedData(fi, out size, true);
                        return new StreamWithRangeValues(new FileStream(_path.FS, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), fi.Offset, size, fi.CompressionType, fi.UncompressedSize);
                    }
                }

                Debug.WriteLine($"ArchiveWorker: NO SUCH FILE! :: Searched {_path} and could not find {fileName}");
                return null;
            }
            return null;
        }

        public bool MoveNext() => (GetListOfFiles()?.Length ?? 0) > 0 && enumerator.MoveNext();

        public void Reset()
        {
            string[] list = GetListOfFiles();
            if (list != null && list.Length > 0)
                enumerator.Reset();
        }

        public override string ToString() => $"{_path} :: {Used}";

        public bool TryGetValue(string key, out byte[] value) => (value = GetBinaryFile(key)) != null ? true : false;

        private static bool GetLine(TextReader tr, out string line)
        {
            line = tr.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
            {
                line.TrimEnd('\0');
                return true;
            }
            return false;
        }



        private int FindFile(ref string filename)
        {
            if (ArchiveMap != null && ArchiveMap.Count > 1)
                return ArchiveMap.FindString(ref filename, out int size) == default ? -1 : 0;
            else if (isDir)
            {
                string f = filename;
                filename = FileList.FirstOrDefault(x => x.IndexOf(f, StringComparison.OrdinalIgnoreCase) >= 0);
                return string.IsNullOrWhiteSpace(filename) ? -1 : 0;
            }
            else
                using (FileStream fs = new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return FindFile(ref filename, fs); //File.OpenRead(_path.FL));
        }

        /// <summary>
        /// Search file list for line filename is on.
        /// </summary>
        /// <param name="filename">filename or path to search for</param>
        /// <param name="stream">stream of text data to search in</param>
        /// <returns>-1 on error or &gt;=0 on success.</returns>
        private int FindFile(ref string filename, Stream stream)
        {
            if (string.IsNullOrWhiteSpace(filename)) return -1;
            filename = filename.TrimEnd('\0');

            using (TextReader tr = new StreamReader(stream))
            {
                for (int i = 0; GetLine(tr, out string line); i++)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Debug.WriteLine("ArchiveWorker::File entry is null. Returning -1");
                        break;
                    }
                    if (line.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        filename = line; //make sure the filename in dictionary is consistant.
                        return i;
                    }
                }
            }
            Debug.WriteLine($"ArchiveWorker:: Filename {filename}, not found. returning -1");
            return -1;
        }

        private byte[] GetBinaryFile(string fileName, int loc, bool cache)
        {
            fileName = FileList.FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
            if (LocalTryGetValue(fileName, out BufferWithAge b))
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: read from cache: {fileName}");
                return b;
            }
            if (isDir)
            {
                if (FileList == null || FileList.Length == 0)
                    ProduceFileLists();
                if (!string.IsNullOrWhiteSpace(fileName))
                    using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: reading: {fileName}");
                        byte[] buffer = br.ReadBytes(checked((int)br.BaseStream.Length));
                        if (cache && LocalTryAdd(fileName, buffer))
                        {
                            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: cached: {fileName}");
                        }
                        return buffer;
                    }
            }
            byte[] temp = null;
            //read index data

            FI FI = GetFI(loc);
            //read binary data.
            temp = GetCompressedData(FI, out int size);

            Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: extracting: {fileName}");
            temp = temp == null ? null : FI.CompressionType == CompressionType.LZ4 ? ArchiveMap.LZ4Uncompress(temp, FI.UncompressedSize) : FI.CompressionType == CompressionType.LZSS ? LZSS.DecompressAllNew(temp) : temp;
            if (temp != null && cache && LocalTryAdd(fileName, temp))
            {
                Memory.Log.WriteLine($"{nameof(ArchiveWorker)}::{nameof(GetBinaryFile)} :: cached: {fileName}");
            }
            return temp;
        }

        private byte[] GetCompressedData(FI FI, out int size, bool skipdata = false)
        {
            byte[] temp = null;
            using (BinaryReader br = new BinaryReader(new FileStream(_path.FS, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                br.BaseStream.Seek(FI.Offset, SeekOrigin.Begin);
                size = FI.CompressionType == CompressionType.LZSS ? br.ReadInt32() : FI.UncompressedSize;//checked((int)(br.BaseStream.Length - br.BaseStream.Position));
                if (!skipdata)
                    temp = br.ReadBytes(size);
            }
            return temp;
        }

        private KeyValuePair<string, byte[]> GetCurrent()
        {
            string s = (string)(enumerator.Current);
            return new KeyValuePair<string, byte[]>(s, GetBinaryFile(s));
        }

        private FI GetFI(int loc)
        {
            using (BinaryReader br = new BinaryReader(new FileStream(_path.FI, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))//File.OpenRead(_path.FI)))
            {
                br.BaseStream.Seek(loc * 12, SeekOrigin.Begin);
                return Extended.ByteArrayToClass<FI>(br.ReadBytes(12));
            }
        }

        /// <summary>
        /// Generate a file list from binary data.
        /// </summary>
        /// <param name="fl">raw File List</param>
        private string[] GetListOfFiles(byte[] fl)
        {
            using (MemoryStream ms = new MemoryStream(fl))
                return ProduceFileLists(ms);
        }

        /// <summary>
        /// Generate a file list from raw text file.
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/12744725/how-do-i-perform-file-readalllines-on-a-file-that-is-also-open-in-excel"/>
        private string[] ProduceFileLists()
        {
            if (_path != null)
            {
                if (isDir)
                    return Directory.GetFiles(_path, "*", SearchOption.AllDirectories).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
                else if (ArchiveMap != null && ArchiveMap.Count > 0)
                    return ArchiveMap.OrderedByName.Select(x => x.Key).ToArray();
                else if (File.Exists(_path.FL))
                    using (FileStream fs = new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        return ProduceFileLists(fs).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            }
            return null;
        }

        private string[] ProduceFileLists(Stream s)
        {
            using (StreamReader sr = new StreamReader(s, System.Text.Encoding.UTF8))
            {
                List<string> fl = new List<string>();
                while (!sr.EndOfStream)
                    fl.Add(sr.ReadLine().Trim('\0', '\r', '\n'));
                return fl.ToArray();
            }
        }

        #endregion Methods
    }
}