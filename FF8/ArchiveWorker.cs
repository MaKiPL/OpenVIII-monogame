using System;
using System.Diagnostics;
using System.IO;

namespace FF8
{
    public class ArchiveWorker
    {
        #region Fields

        /// <summary>
        /// Generated File List
        /// </summary>
        public string[] FileList;
        private bool _compressed;
        private uint _locationInFs;
        private Memory.Archive _path;
        private uint _unpackedFileSize;

        #endregion Fields

        #region Constructors

        public ArchiveWorker(Memory.Archive path, bool skiplist = false)
        {
            _path = path;
            if(!skiplist)
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
        public static byte[] GetBinaryFile(Memory.Archive archive, string fileName)
        {
            ArchiveWorker tmp = new ArchiveWorker(archive,true);
            return tmp.GetBinaryFile(fileName);
        }

        /// <summary>
        /// Give me three archives as bytes uncompressed please!
        /// </summary>
        /// <param name="FI">FileIndex</param>
        /// <param name="FS">FileSystem</param>
        /// <param name="FL">FileList</param>
        /// <param name="filename">Filename of the file to get</param>
        /// <returns></returns>
        /// <remarks>Does the same thing as Get Binary file, but it reads from byte arrays in ram
        /// because the data was already pulled from a file earlier.</remarks>
        public byte[] FileInTwoArchives(byte[] FI, byte[] FS, byte[] FL, string filename)
        {
            filename = filename.TrimEnd('\0');
            string flText = System.Text.Encoding.UTF8.GetString(FL).Replace(Convert.ToString(0x0d), "");
            int loc = -1;
            string[] files = flText.Split((char)0x0a);
            //check archive for filename
            for (int i = 0; i != files.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(files[i]))
                {
                    Debug.WriteLine("ArchiveWorker::File entry is null. Returning null");
                    return null;
                }
                string testme = files[i].Substring(0, files[i].Length - 1).TrimEnd('\0');

                if (testme.IndexOf(filename, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    loc = i;
                    break;
                }
            }
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
        public byte[] GetBinaryFile(string fileName)
        {
            if (fileName.Length < 1)
                throw new FileNotFoundException("NO FILENAME");

            int loc = -1;
            byte[] temp = null;
            // read file list
            using (TextReader tr = new StreamReader(new FileStream(_path.FL, FileMode.Open)))
            {
                string locTr = tr.ReadToEnd();
                string[] files = locTr.Split((char)0x0a);
                for (int i = 0; i != files.Length - 1; i++)
                {
                    string testme = files[i].Substring(0, files[i].Length - 1);
                    if (testme.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        loc = i;
                        break;
                    }
                }
            }
            if (loc == -1)
            {
                Debug.WriteLine("ArchiveWorker: NO SUCH FILE!");
                //throw new Exception("ArchiveWorker: No such file!");
            }
            else
            {
                //read index data
                using (BinaryReader br = new BinaryReader(new FileStream(_path.FI, FileMode.Open)))
                {
                    br.BaseStream.Seek(loc * 12, SeekOrigin.Begin);
                    _unpackedFileSize = br.ReadUInt32(); //fs.Seek(4, SeekOrigin.Current);
                    _locationInFs = br.ReadUInt32();
                    _compressed = br.ReadUInt32() != 0;
                }
                //read binary data.
                using (BinaryReader br = new BinaryReader(new FileStream(_path.FS, FileMode.Open)))
                {
                    br.BaseStream.Seek(_locationInFs, SeekOrigin.Begin);
                    temp = br.ReadBytes(_compressed ? br.ReadInt32() : (int)_unpackedFileSize);
                }
            }
            if (temp == null) throw new FileNotFoundException($"Searched {_path} and could not find {fileName}.", fileName);

            return temp == null ? null : _compressed ? LZSS.DecompressAllNew(temp) : temp;
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
        /// Generate a file list from raw text file.
        /// </summary>
        private string[] ProduceFileLists() => File.ReadAllLines(_path.FL, System.Text.Encoding.ASCII);

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