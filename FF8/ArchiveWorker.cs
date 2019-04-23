using System;
using System.Diagnostics;
using System.IO;

namespace FF8
{
    internal class ArchiveWorker
    {
        static uint _unpackedFileSize;
        static uint _locationInFs;
        static bool _compressed;
        private string _path;
        public static string[] FileList;


        public ArchiveWorker(string path)
        {
            _path = MakiExtended.GetUnixFullPath(path);
            string root = Path.GetDirectoryName(_path);
            string file = Path.GetFileNameWithoutExtension(path);
            string fi = MakiExtended.GetUnixFullPath($"{Path.Combine(root, file)}{Memory.Archives.B_FileIndex}");
            string fl = MakiExtended.GetUnixFullPath($"{Path.Combine(root, file)}{Memory.Archives.B_FileList}");
            if (!File.Exists(fi)) throw new Exception($"There is no {file}.fi file!\nExiting...");
            if (!File.Exists(fl)) throw new Exception($"There is no {file}.fl file!\nExiting...");
            FileList = ProduceFileLists();
        }

        private string[] ProduceFileLists() =>
            File.ReadAllLines(
                    $"{Path.Combine(Path.GetDirectoryName(_path), Path.GetFileNameWithoutExtension(_path))}{Memory.Archives.B_FileList}"
                    );

        public static string[] GetBinaryFileList(byte[] fl) =>System.Text.Encoding.ASCII.GetString(fl).Replace("\r", "").Replace("\0", "").Split('\n');

        public byte[] GetBinaryFile(string fileName) => GetBinaryFile(_path, fileName);
        public static byte[] GetBinaryFile(string archiveName, string fileName)
        {
            byte[] isComp = GetBin(MakiExtended.GetUnixFullPath(archiveName), fileName);
            return isComp == null ? null : _compressed ? LZSS.DecompressAll(isComp, (uint)isComp.Length, (int)_unpackedFileSize) : isComp;
        }
        /// <summary>
        /// Give me three archives as bytes uncompressed please!
        /// </summary>
        /// <param name="FI">FileIndex</param>
        /// <param name="FS">FileSystem</param>
        /// <param name="FL">FileList</param>
        /// <param name="filename">Filename of the file to get</param>
        /// <returns></returns>
        public static byte[] FileInTwoArchives(byte[] FI, byte[] FS, byte[] FL, string filename)
        {
            string a = filename.TrimEnd('\0');

            string flText = System.Text.Encoding.UTF8.GetString(FL);
            flText = flText.Replace(Convert.ToString(0x0d), "");
            int loc = -1;
            string[] files = flText.Split((char)0x0a);
            for (int i = 0; i != files.Length; i++) //check archive for filename
            {
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
            return compe ? LZSS.DecompressAll(file, (uint)file.Length, (int)fsLen) : file;
        }

        private static byte[] GetBin(string archiveName, string fileName)
        {
            if (fileName.Length < 1 || archiveName.Length < 1)
                throw new System.Exception("NO FILENAME OR ARCHIVE!");

            string archivePath = archiveName + Memory.Archives.B_FileArchive;
            string archiveIndexPath = archiveName + Memory.Archives.B_FileIndex;
            string archiveNamesPath = archiveName + Memory.Archives.B_FileList;
            int loc = -1;

            FileStream fs = new FileStream(archiveNamesPath, FileMode.Open);
            TextReader tr = new StreamReader(fs);
            string locTr = tr.ReadToEnd();
            tr.Dispose();
            fs.Close();
            //locTr = locTr.Replace(Convert.ToString(0x0d), "");
            string[] files = locTr.Split((char)0x0a);
            for (int i = 0; i != files.Length - 1; i++)
            {
                string testme = files[i].Substring(0, files[i].Length - 1).ToUpper();
                if (testme == fileName.ToUpper())
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

            fs = new FileStream(archiveIndexPath, FileMode.Open);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(loc * 12, SeekOrigin.Begin);
            _unpackedFileSize = br.ReadUInt32(); //fs.Seek(4, SeekOrigin.Current);
            _locationInFs = br.ReadUInt32();
            _compressed = br.ReadUInt32() != 0;
            fs.Close();

            fs = new FileStream(archivePath, FileMode.Open);
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
            string flPath = $"{Path.GetDirectoryName(_path)}\\{Path.GetFileNameWithoutExtension(_path)}.fi";
            using (FileStream fs = new FileStream(flPath, FileMode.Open, FileAccess.Read))
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