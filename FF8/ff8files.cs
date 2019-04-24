using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FF8
{
    internal static class Ff8files
    {
        public struct Data
        {
            public ushort LocationID;//0x0004
            public ushort firstcharacterscurrentHP;//0x0006
            public ushort firstcharactersmaxHP;//0x0008
            public ushort savecount;//0x000A
            public uint AmountofGil;//0x000C
            public uint Totalnumberofsecondsplayed;//0x0020
            public byte firstcharacterslevel;//0x0024
            public byte firstcharactersportrait;//0x0025
            public byte secondcharactersportrait;//0x0026
            public byte thirdcharactersportrait;//0x0027
            public byte[] Squallsname;//0x0028 //12
            public byte[] Rinoasname;//0x0034 //12
            public byte[] Angelosname;//0x0040 //12
            public byte[] Bokosname;//0x004C
            public uint CurrentDisk;//0x0058
            public uint Currentsave;//0x005C
        }
        //C:\Users\pcvii\OneDrive\Documents\Square Enix\FINAL FANTASY VIII Steam\user_1727519

        public static string SaveFolder { get; private set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Square Enix", "FINAL FANTASY VIII Steam");
        public static Data[,] FileList { get; private set; }

        public static void init()
        {
            SaveFolder = Directory.GetDirectories(SaveFolder)[0];
            FileList = new Data[2, 30];
            foreach (string file in Directory.EnumerateFiles(SaveFolder))
            {
                Match n = Regex.Match(file, @"slot(\d+)_save(\d+).ff8");
                    
                if(n.Success && n.Groups.Count>0)
                {
                    FileList[int.Parse(n.Groups[1].Value)-1, int.Parse(n.Groups[2].Value)-1] = read(file);
                }
            }
        }

        private static Data read(string file)
        {
            byte[] decmp;

            using (FileStream fs = File.OpenRead(file))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint size = br.ReadUInt32();
                //uint fsLen = BitConverter.ToUInt32(FI, loc * 12);
                //uint fSpos = BitConverter.ToUInt32(FI, (loc * 12) + 4);
                //bool compe = BitConverter.ToUInt32(FI, (loc * 12) + 8) != 0;
                //fs.Seek(0, SeekOrigin.Begin);
                byte[] tmp = br.ReadBytes((int)fs.Length-4);
                decmp = LZSS.DecompressAll(tmp,(uint)fs.Length-4);
            }
            using (FileStream fs = File.Create(Path.Combine(@"d:\", Path.GetFileName(file))))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(decmp);
            }
            using (MemoryStream ms = new MemoryStream(decmp))
            using (BinaryReader br = new BinaryReader(ms))
            {
                Data d = new Data();
                ms.Seek(0x0004, SeekOrigin.Begin); d.LocationID = br.ReadUInt16();//0x0004
                ms.Seek(0x0006, SeekOrigin.Begin); d.firstcharacterscurrentHP = br.ReadUInt16();//0x0006
                ms.Seek(0x0008, SeekOrigin.Begin); d.firstcharactersmaxHP = br.ReadUInt16();//0x0008
                ms.Seek(0x000A, SeekOrigin.Begin); d.savecount = br.ReadUInt16();//0x000A
                ms.Seek(0x000C, SeekOrigin.Begin); d.AmountofGil = br.ReadUInt32();//0x000C
                ms.Seek(0x0020, SeekOrigin.Begin); d.Totalnumberofsecondsplayed = br.ReadUInt32();//0x0020
                ms.Seek(0x0024, SeekOrigin.Begin); d.firstcharacterslevel = br.ReadByte();//0x0024
                ms.Seek(0x0025, SeekOrigin.Begin); d.firstcharactersportrait = br.ReadByte();//0x0025
                ms.Seek(0x0026, SeekOrigin.Begin); d.secondcharactersportrait = br.ReadByte();//0x0026
                ms.Seek(0x0027, SeekOrigin.Begin); d.thirdcharactersportrait = br.ReadByte();//0x0027
                ms.Seek(0x0028, SeekOrigin.Begin); d.Squallsname = br.ReadBytes(12);//0x0028
                ms.Seek(0x0034, SeekOrigin.Begin); d.Rinoasname = br.ReadBytes(12);//0x0034
                ms.Seek(0x0040, SeekOrigin.Begin); d.Angelosname = br.ReadBytes(12);//0x0040
                ms.Seek(0x004C, SeekOrigin.Begin); d.Bokosname = br.ReadBytes(12);//0x004C
                ms.Seek(0x0058, SeekOrigin.Begin); d.CurrentDisk = br.ReadUInt32();//0x0058
                ms.Seek(0x005C, SeekOrigin.Begin); d.Currentsave = br.ReadUInt32();//0x005C
                return d;
            }
        }
    }
}