using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OpenVIII
{
    public class ArchiveWorker
    {
        #region Fields

        public Memory.Archive _path;

        /// <summary>
        /// Generated File List
        /// </summary>
        public string[] FileList;

        private static Dictionary<Memory.Archive, Dictionary<string, byte[]>> ArchiveCache = new Dictionary<Memory.Archive, Dictionary<string, byte[]>>
            {
                { Memory.Archives.A_BATTLE, new Dictionary<string, byte[]>() },
                { Memory.Archives.A_FIELD, new Dictionary<string, byte[]>() },
                { Memory.Archives.A_MAGIC, new Dictionary<string, byte[]>() },
                { Memory.Archives.A_MAIN, new Dictionary<string, byte[]>() },
                { Memory.Archives.A_MENU, new Dictionary<string, byte[]>() },
                { Memory.Archives.A_WORLD, new Dictionary<string, byte[]>() }
            };

        /// <summary>
        /// prevent two threads from writing to cache at the same time.
        /// </summary>
        private static object cachelock = new object();

        private bool _compressed;
        private uint _locationInFs;
        private uint _unpackedFileSize;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Saves the active archive and file list.
        /// </summary>
        /// <param name="path">Memory.Archive</param>
        /// <param name="skiplist">If list generation is unneeded you can skip it by setting true</param>
        public ArchiveWorker(Memory.Archive path, bool skiplist = false)
        {
            _path = path;
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
        public byte[] FileInTwoArchives(byte[] FI, byte[] FS, byte[] FL, string filename)
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
        /// GetBinary
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] GetBinaryFile(string fileName, bool cache = false)
        {
            if (fileName.Length < 1)
                throw new FileNotFoundException("NO FILENAME");

            int loc = FindFile(ref fileName, new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)); //File.OpenRead(_path.FL));

            // read file list

            if (loc == -1)
            {
                Debug.WriteLine("ArchiveWorker: NO SUCH FILE!");
                //throw new Exception("ArchiveWorker: No such file!");
            }
            else
            {
                if (cache)
                    lock (cachelock)
                    {
                        return GetBinaryFile(fileName, loc, cache);
                    }
                else
                    return GetBinaryFile(fileName, loc, cache);
            }
            throw new FileNotFoundException($"Searched {_path} and could not find {fileName}.", fileName);
        }

        /// <summary>
        /// Generate a file list from binary data.
        /// </summary>
        /// <param name="fl"></param>
        public string[] GetBinaryFileList(byte[] fl) => System.Text.Encoding.ASCII.GetString(fl).Replace("\r", "").Replace("\0", "").Split('\n');

        /// <summary>
        /// Get current file list for loaded archive.
        /// </summary>
        public string[] GetListOfFiles() => FileList;

        public Memory.Archive GetPath() => _path;

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

        private byte[] GetBinaryFile(string fileName, int loc, bool cache)
        {
            byte[] temp = null;
            if (ArchiveCache.ContainsKey(_path) && ArchiveCache[_path].ContainsKey(fileName))
                return (ArchiveCache[_path][fileName]);
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
            if (temp != null && cache) ArchiveCache[_path][fileName] = temp;
            return temp;
        }

        /// <summary>
        /// Generate a file list from raw text file.
        /// </summary>
        /// <see cref="https://stackoverflow.com/questions/12744725/how-do-i-perform-file-readalllines-on-a-file-that-is-also-open-in-excel"/>
        private string[] ProduceFileLists()
        {
            //return File.ReadAllLines(_path.FL, System.Text.Encoding.ASCII);
            using (StreamReader sr = new StreamReader(new FileStream(_path.FL, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), System.Text.Encoding.ASCII))
            {
                List<string> fl = new List<string>();
                while (!sr.EndOfStream)
                    fl.Add(sr.ReadLine());
                return fl.ToArray();
            }
        }

        #endregion Methods

        //public struct FI
        //{
        //    public uint LengthOfUnpackedFile;
        //    public uint LocationInFS;
        //    public uint LZSS;
        //}

        //public FI[] GetFI()
        //{
        //    FI[] FileIndex = new FI[FileList.Length];
        //    using (FileStream fs = new FileStream(_path.FI, FileMode.Open, FileAccess.Read))
        //    using (BinaryReader br = new BinaryReader(fs))
        //        for (int i = 0; i <= FileIndex.Length - 1; i++)
        //        {
        //            FileIndex[i].LengthOfUnpackedFile = br.ReadUInt32();
        //            FileIndex[i].LocationInFS = br.ReadUInt32();
        //            FileIndex[i].LZSS = br.ReadUInt32();
        //        }
        //    return FileIndex;
        //}
    }
}