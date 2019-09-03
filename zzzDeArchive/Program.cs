using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;

namespace zzzDeArchive
{
    public class Program
    {
        private const string _path = @"D:\ext2";
        private const string _out = @"D:\out.zzz";

        public struct ZzzHeader
        {
            public uint Count;
            public FileData[] Data;

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
            public static ZzzHeader Read(string path)
            {
                ZzzHeader r = new ZzzHeader();
                var f = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                r.Count = (uint)f.Length;
                r.Data = new FileData[r.Count];
                
                for(int i = 0; i < r.Count; i++)
                {
                    string safe = f[i];
                    safe = safe.Replace(path, "");
                    safe = safe.Replace('/', '\\');
                    safe = safe.Trim('\\');
                    r.Data[i].Filename = safe;
                    FileInfo fi = new FileInfo(f[i]);
                    r.Data[i].Size = (uint)fi.Length;
                }
                uint pos = (uint)r.TotalBytes;
                for (int i = 0; i < r.Count; i++)
                {
                    r.Data[i].Offset = pos;
                    pos += r.Data[i].Size;
                }
                return r;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(Count);
                foreach (FileData r in Data)
                    r.Write(bw);
            }
            public int TotalBytes => sizeof(uint) + (from x in Data select x.TotalBytes).Sum();
            public override string ToString() => $"({Count} files)";
        }

        public struct FileData
        {
            public uint FilenameLength;
            private byte[] filenameascii;
            public uint Offset;
            public uint UnkFlag;//Boolean
            public uint Size;

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

            public void Write(BinaryWriter bw)
            {
                bw.Write(FilenameLength);
                bw.Write(filenameascii);
                bw.Write(Offset);
                bw.Write(UnkFlag);
                bw.Write(Size);
            }

            public string Filename
            {
                get => Encoding.ASCII.GetString(filenameascii); set
                {
                    filenameascii = Encoding.ASCII.GetBytes(value);
                    FilenameLength = (uint)filenameascii.Length;
                }
            }
            public int TotalBytes => (int)(sizeof(uint) * 4 + FilenameLength);
            public override string ToString() => $"({Filename}, {Offset}, {UnkFlag}, {Size})";
        }

        private static void Main(string[] args)
        {
            //Extract();
            Compile();
            Console.ReadLine();
        }

        private static void Compile()
        {
            ZzzHeader head = ZzzHeader.Read(_path);
        }

        private static void Extract()
        {
            //string filename = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII Remastered\main.zzz";
            string filename = @"C:\Program Files (x86)\Steam\steamapps\common\FINAL FANTASY VIII Remastered\other.zzz";
            ZzzHeader head;
            using (FileStream fs = File.OpenRead(filename))
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
    }
}