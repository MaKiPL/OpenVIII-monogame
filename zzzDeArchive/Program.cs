using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace zzzDeArchive
{
    public class Program
    {
        #region Fields

        //string filename = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII Remastered\main.zzz";
        private const string _in = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII Remastered\other.zzz";

        private const string _out = @"out.zzz";
        private const string _path = @"D:\ext";

        #endregion Fields

        #region Methods

        private static void Write()
        {
            ZzzHeader head = ZzzHeader.Read(_path, out string[] f);

            Console.WriteLine(head);
            using (FileStream fs = File.Create(_out))
            {
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    head.Write(bw);
                    foreach (string file in f)
                    {
                        bw.Write(File.ReadAllBytes(file));
                    }
                }
            }
        }

        private static void Extract()
        {
            ZzzHeader head;
            using (FileStream fs = File.OpenRead(_in))
            {
                using (BinaryReader br = new BinaryReader(fs))
                {
                    head = ZzzHeader.Read(br);
                    Console.WriteLine(head);

                    Directory.CreateDirectory(_path);
                    foreach (FileData d in head.Data)
                    {
                        Debug.Assert(d.UnkFlag == 0);
                        Console.WriteLine($"Writing {d}");
                        string path = Path.Combine(_path, d.Filename);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                        using (FileStream fso = File.Create(path))
                        {
                            using (BinaryWriter bw = new BinaryWriter(fso))
                            {
                                fs.Seek(d.Offset, SeekOrigin.Begin);
                                bw.Write(br.ReadBytes((int)d.Size));
                            }
                        }
                    }
                }
            }
        }

        private static void Main(string[] args)
        {
            //Extract();
            Write();
            Console.ReadLine();
        }

        #endregion Methods

        #region Structs

        public struct FileData
        {
            #region Fields

            private byte[] filenameascii;
            public uint FilenameLength;
            public uint Offset;
            public uint Size;
            public uint UnkFlag;

            #endregion Fields

            #region Properties

            //Boolean
            public string Filename
            {
                get => Encoding.ASCII.GetString(filenameascii); set
                {
                    filenameascii = Encoding.ASCII.GetBytes(value);
                    FilenameLength = (uint)filenameascii.Length;
                }
            }

            public int TotalBytes => (int)(sizeof(uint) * 4 + FilenameLength);

            #endregion Properties

            #region Methods

            public static FileData Read(BinaryReader br)
            {
                FileData r = new FileData
                {
                    FilenameLength = br.ReadUInt32()
                };
                r.filenameascii = br.ReadBytes((int)r.FilenameLength);
                r.Offset = br.ReadUInt32();
                r.UnkFlag = br.ReadUInt32();
                r.Size = br.ReadUInt32();
                return r;
            }

            public static FileData Read(string path)
            {
                string safe = path;
                safe = safe.Replace(_path, "");
                safe = safe.Replace('/', '\\');
                safe = safe.Trim('\\');
                FileInfo fi = new FileInfo(path);
                FileData r = new FileData
                {
                    Filename = safe,
                    Size = (uint)fi.Length
                };
                return r;
            }

            public override string ToString() => $"({Filename}, {Offset}, {UnkFlag}, {Size})";

            public void Write(BinaryWriter bw)
            {
                bw.Write(FilenameLength);
                bw.Write(filenameascii);
                bw.Write(Offset);
                bw.Write(UnkFlag);
                bw.Write(Size);
            }

            #endregion Methods
        }

        public struct ZzzHeader
        {
            #region Fields

            public uint Count;
            public FileData[] Data;

            #endregion Fields

            #region Properties

            public int TotalBytes => sizeof(uint) + (from x in Data select x.TotalBytes).Sum();

            #endregion Properties

            #region Methods

            public static ZzzHeader Read(BinaryReader br)
            {
                ZzzHeader r = new ZzzHeader
                {
                    Count = br.ReadUInt32()
                };
                r.Data = new FileData[r.Count];
                for (int i = 0; i < r.Count; i++)
                    r.Data[i] = FileData.Read(br);
                return r;
            }

            /// <summary>
            /// This creates the header using the files in a directory.
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static ZzzHeader Read(string path, out string[] f)
            {
                ZzzHeader r = new ZzzHeader();
                f = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                r.Count = (uint)f.Length;
                r.Data = new FileData[r.Count];

                for (int i = 0; i < r.Count; i++)
                    r.Data[i] = FileData.Read(f[i]);

                uint pos = (uint)r.TotalBytes;

                //cannot know the size of the header till i had loaded the rest of the data.
                //so now we are updating the offset to be past the header. in the same order as the files.
                for (int i = 0; i < r.Count; i++)
                {
                    r.Data[i].Offset = pos;
                    pos += r.Data[i].Size;
                }
                return r;
            }

            public override string ToString() => $"({Count} files)";

            public void Write(BinaryWriter bw)
            {
                bw.Write(Count);
                foreach (FileData r in Data)
                {
                    Console.WriteLine($"Writing {r}");
                    r.Write(bw);
                }
            }

            #endregion Methods
        }

        #endregion Structs
    }
}