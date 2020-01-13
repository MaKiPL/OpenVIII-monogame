using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class ArchiveWorker : ArchiveBase
    {
        #region Fields

        public Memory.Archive _path;

        /// <summary>
        /// Generated File List
        /// </summary>
        public string[] FileList;

        private static ConcurrentDictionary<Memory.Archive, ConcurrentDictionary<string, byte[]>> ArchiveCache =
            new ConcurrentDictionary<Memory.Archive, ConcurrentDictionary<string, byte[]>>
                (new Dictionary<Memory.Archive, ConcurrentDictionary<string, byte[]>> {
                { Memory.Archives.A_BATTLE, new ConcurrentDictionary<string, byte[]>() },
                { Memory.Archives.A_FIELD, new ConcurrentDictionary<string, byte[]>() },
                { Memory.Archives.A_MAGIC, new ConcurrentDictionary<string, byte[]>() },
                { Memory.Archives.A_MAIN, new ConcurrentDictionary<string, byte[]>() },
                { Memory.Archives.A_MENU, new ConcurrentDictionary<string, byte[]>() },
                { Memory.Archives.A_WORLD, new ConcurrentDictionary<string, byte[]>() }
            });

        /// <summary>
        /// prevent two threads from writing to cache at the same time.
        /// </summary>
        private static object cachelock = new object();

        private bool _compressed;
        private uint _locationInFs;
        private uint _unpackedFileSize;
        private byte[] FI, FS, FL;
        private bool isDir = false;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Saves the active archive and file list.
        /// </summary>
        /// <param name="path">Memory.Archive</param>
        /// <param name="skiplist">If list generation is unneeded you can skip it by setting true</param>
        public ArchiveWorker(Memory.Archive path, bool skiplist = false)
        {
            if (Directory.Exists(path))
            {
                isDir = true;
            }
            _path = path;
            if (!skiplist)
                FileList = ProduceFileLists();
        }

        public ArchiveWorker(byte[] fI, byte[] fS, byte[] fL, bool skiplist = false)
        {
            FI = fI;
            FS = fS;
            FL = fL;
            if (!skiplist)
                FileList = ProduceFileLists();
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Generate archive worker and get file
        /// </summary>
        /// <param name="archive">which archive you want to read from</param>
        /// <param name="fileName">name of file you want to recieve</param>
        /// <returns>Uncompressed binary file data</returns>
        public static byte[] GetBinaryFile(Memory.Archive archive, string fileName, bool cache = false)
        {
            ArchiveWorker tmp = new ArchiveWorker(archive, true);
            return tmp.GetBinaryFile(fileName, cache);
        }

        public override ArchiveBase GetArchive(Memory.Archive archive) => new ArchiveWorker(GetBinaryFile(archive.FI), GetBinaryFile(archive.FS), GetBinaryFile(archive.FL)) { _path = archive };

        /// <summary>
        /// GetBinary
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new FileNotFoundException("NO FILENAME");

            if (FL != null && FL.Length > 0 && FI != null && FL.Length > 0 && FI != null && FL.Length > 0)
                return FileInTwoArchives(fileName);
            else
            if (!isDir)
            {
                int loc = FindFile(ref fileName, new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); //File.OpenRead(_path.FL));

                // read file list

                if (loc == -1)
                {
                    Debug.WriteLine("ArchiveWorker: NO SUCH FILE!");
                    //throw new Exception("ArchiveWorker: No such file!");
                }
                else
                    return GetBinaryFile(fileName, loc, cache);
            }
            else
                return GetBinaryFile(fileName, 0, cache);
            throw new FileNotFoundException($"Searched {_path} and could not find {fileName}.", fileName);
        }

        //public Stream GetBinaryFileStream(string fileName, bool cache = false) => throw new NotImplementedException();

        /// <summary>
        /// Get current file list for loaded archive.
        /// </summary>
        public override string[] GetListOfFiles() => FileList;

        public override Memory.Archive GetPath() => _path;

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

        /// <summary>
        /// Give me three archives as bytes uncompressed please!
        /// </summary>
        /// <param name="FI">FileIndex</param>
        /// <param name="FS">FileSystem</param>
        /// <param name="FL">FileList</param>
        /// <param name="filename">Filename of the file to get</param>
        /// <returns></returns>
        /// <remarks>
        /// Does the same thing as Get Binary file, but it reads from byte arrays in ram because the
        /// data was already pulled from a file earlier.
        /// </remarks>
        private byte[] FileInTwoArchives(string filename)
        {
            int loc = FindFile(ref filename, new MemoryStream(FL, false));
            if (loc == -1)
            {
                Debug.WriteLine("ArchiveWorker: NO SUCH FILE!");
                return null;
                //throw new Exception("ArchiveWorker: No such file!");
            }

            // get params from index
            uint fsLen = BitConverter.ToUInt32(FI, loc * 12);
            uint fSpos = BitConverter.ToUInt32(FI, (loc * 12) + 4);
            bool compe = BitConverter.ToUInt32(FI, (loc * 12) + 8) != 0;

            // copy binary data
            byte[] file = new byte[fsLen];
            Array.Copy(FS, fSpos, file, 0, file.Length);
            return compe ? LZSS.DecompressAllNew(file) : file;
        }

        /// <summary>
        /// Search file list for line filename is on.
        /// </summary>
        /// <param name="filename">filename or path to search for</param>
        /// <param name="stream">stream of text data to search in</param>
        /// <returns>-1 on error or &gt;=0 on success.</returns>
        private int FindFile(ref string filename, Stream stream)
        {
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
            if (ArchiveCache.TryGetValue(_path, out ConcurrentDictionary<string, byte[]> a) && a.TryGetValue(fileName, out byte[] b))
                return b;
            if (isDir)
            {
                if (FileList == null || FileList.Length == 0)
                    ProduceFileLists();
                fileName = FileList.FirstOrDefault(x => x.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrWhiteSpace(fileName))
                    using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        return br.ReadBytes(checked((int)br.BaseStream.Length));
                    }
            }
            byte[] temp = null;
            //read index data
            using (BinaryReader br = new BinaryReader(new FileStream(_path.FI, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))//File.OpenRead(_path.FI)))
            {
                br.BaseStream.Seek(loc * 12, SeekOrigin.Begin);
                _unpackedFileSize = br.ReadUInt32(); //fs.Seek(4, SeekOrigin.Current);
                _locationInFs = br.ReadUInt32();
                _compressed = br.ReadUInt32() != 0;
            }
            //read binary data.
            using (BinaryReader br = new BinaryReader(new FileStream(_path.FS, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))//File.OpenRead(_path.FS)))
            {
                br.BaseStream.Seek(_locationInFs, SeekOrigin.Begin);
                temp = br.ReadBytes(_compressed ? br.ReadInt32() : (int)_unpackedFileSize);
            }

            temp = temp == null ? null : _compressed ? LZSS.DecompressAllNew(temp) : temp;
            if (temp != null && cache && ArchiveCache.TryGetValue(_path, out a))
                a.TryAdd(fileName, temp);
            return temp;
        }

        /// <summary>
        /// Generate a file list from binary data.
        /// </summary>
        /// <param name="fl">raw File List</param>
        private string[] GetListOfFiles(byte[] fl) => ProduceFileLists(new MemoryStream(fl));

        /// <summary>
        /// Generate a file list from raw text file.
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/12744725/how-do-i-perform-file-readalllines-on-a-file-that-is-also-open-in-excel"/>
        private string[] ProduceFileLists()
        {
            if (isDir)
                return Directory.GetFiles(_path, "*", SearchOption.AllDirectories).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
            if (FL != null && FL.Length > 0)
                return GetListOfFiles(FL);
            return ProduceFileLists(new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).OrderBy(x => x.Length).ThenBy(x => x, StringComparer.OrdinalIgnoreCase).ToArray();
        }

        private string[] ProduceFileLists(Stream s)
        {
            using (StreamReader sr = new StreamReader(s, System.Text.Encoding.ASCII))
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