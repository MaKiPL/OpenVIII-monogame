using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FF8
{
    public class ArchiveWorker
    {
        uint _unpackedFileSize;
        uint _locationInFs;
        bool _compressed;
        private Memory.Archive _path;
        public Memory.Archive GetPath() => _path;
        public string[] FileList;


        public ArchiveWorker(Memory.Archive path)
        {
            _path = path;
            FileList = ProduceFileLists();
        }

        private string[] ProduceFileLists() => File.ReadAllLines(_path.FL);

        public string[] GetBinaryFileList(byte[] fl) =>System.Text.Encoding.ASCII.GetString(fl).Replace("\r", "").Replace("\0", "").Split('\n');

        public byte[] GetBinaryFile(string fileName)
        {
            byte[] isComp = GetBin(_path, fileName);
            if (isComp == null) throw new FileNotFoundException($"Searched {_path} and could not find {fileName}.",fileName);
            if(_compressed)
                isComp = isComp.Skip(4).ToArray();
            return isComp == null ? null : _compressed ? LZSS.DecompressAllNew(isComp) : isComp;
        }
        static public byte[] GetBinaryFile(Memory.Archive archive, string fileName)
        {
            var tmp = new ArchiveWorker(archive);
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
        public byte[] FileInTwoArchives(byte[] FI, byte[] FS, byte[] FL, string filename)
        {
            string a = filename.TrimEnd('\0');

            string flText = System.Text.Encoding.UTF8.GetString(FL);
            flText = flText.Replace(Convert.ToString(0x0d), "");
            int loc = -1;
            string[] files = flText.Split((char)0x0a);
            for (int i = 0; i != files.Length; i++) //check archive for filename
            {
                if(string.IsNullOrWhiteSpace(files[i]))
                {
                    Debug.WriteLine("ArchiveWorker::File entry is null. Returning null");
                    return null;
                }
                string testme = files[i].Substring(0, files[i].Length - 1).ToUpper().TrimEnd('\0');
                if (testme != a.ToUpper()) continue;
                loc = i;
                break;
            }
            if (loc == -1)
            {
                Debug.WriteLine("ArchiveWorker: NO SUCH FILE!");
                return null;
                //throw new Exception("ArchiveWorker: No such file!");
            }


            uint fsLen = BitConverter.ToUInt32(FI, loc * 12);
            uint fSpos = BitConverter.ToUInt32(FI, (loc * 12) + 4);
            bool compe = BitConverter.ToUInt32(FI, (loc * 12) + 8) != 0;
            
            byte[] file = new byte[fsLen];

            Array.Copy(FS, fSpos, file, 0, file.Length);
            return compe ? LZSS.DecompressAllNew(file) : file;
        }

        private byte[] GetBin(Memory.Archive archive, string fileName)
        {
            if (fileName.Length < 1)
                throw new FileNotFoundException("NO FILENAME");

            int loc = -1;

            FileStream fs = new FileStream(archive.FL, FileMode.Open);
            TextReader tr = new StreamReader(fs);
            string locTr = tr.ReadToEnd();
            tr.Dispose();
            fs.Close();
            //locTr = locTr.Replace(Convert.ToString(0x0d), "");
            string[] files = locTr.Split((char)0x0a);
            for (int i = 0; i != files.Length - 1; i++)
            {
                string testme = files[i].Substring(0, files[i].Length - 1);
                if (testme.IndexOf(fileName,StringComparison.OrdinalIgnoreCase)>=0)
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

            fs = new FileStream(archive.FI, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(loc * 12, SeekOrigin.Begin);
            _unpackedFileSize = br.ReadUInt32(); //fs.Seek(4, SeekOrigin.Current);
            _locationInFs = br.ReadUInt32();
            _compressed = br.ReadUInt32() != 0;
            fs.Close();

            fs = new FileStream(archive.FS, FileMode.Open);
            fs.Seek(_locationInFs, SeekOrigin.Begin);

            br = new BinaryReader(fs);
            int howMany = _compressed ? br.ReadInt32() : (int)_unpackedFileSize;

            byte[] temp;
            if (_compressed)
            {
                fs.Seek(-4, SeekOrigin.Current);
                temp = br.ReadBytes(howMany + 4);

            }
            else
                temp = br.ReadBytes(howMany);

            fs.Close();


            return temp;
        }

        public string[] GetListOfFiles() => FileList;

        public struct FI
        {
            public uint LengthOfUnpackedFile;
            public uint LocationInFS;
            public uint LZSS;
        }

        public FI[] GetFI()
        {
            FI[] FileIndex = new FI[FileList.Length];
            using (FileStream fs = new FileStream(_path.FI, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
                for (int i = 0; i <= FileIndex.Length - 1; i++)
                {
                    FileIndex[i].LengthOfUnpackedFile = br.ReadUInt32();
                    FileIndex[i].LocationInFS = br.ReadUInt32();
                    FileIndex[i].LZSS = br.ReadUInt32();
                }
            return FileIndex;
        }
    }
}