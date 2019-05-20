using System;
using System.IO;
using System.Text.RegularExpressions;

namespace FF8
{
    // ref http://wiki.ffrtt.ru/index.php/FF8/Variables copied the table into excel and tried to
    // changed the list into c# code. I was thinking atleast some of it would be useful.




    /// <summary>
    /// parse data from save game files
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php/FF8/GameSaveFormat#The_save_format"/>
    /// <seealso cref="https://github.com/myst6re/hyne"/>
    /// <seealso cref="https://github.com/myst6re/hyne/blob/master/SaveData.h"/>
    /// <seealso cref="https://cdn.discordapp.com/attachments/552838120895283210/570733614656913408/ff8_save.zip"/>
    /// <remarks>
    /// antiquechrono was helping. he even wrote a whole class using kaitai. Though I donno if we
    /// wanna use kaitai.
    /// </remarks>
    public static partial class Saves
    {

        //C:\Users\[user]\OneDrive\Documents\Square Enix\FINAL FANTASY VIII Steam\user_#######
        // might crash linux. just trying to get something working.
        public static string SteamFolder { get; private set; }
        public static string CD2000Folder { get; private set; }

        public static Data[,] FileList { get; private set; }

        public static void Init()
        {
            FileList = new Data[2, 30];
            CD2000Folder = Path.Combine(Memory.FF8DIR, "Save");
            SteamFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Square Enix", "FINAL FANTASY VIII Steam");
            if (Directory.Exists(SteamFolder))
            {
                string[] dirs = Directory.GetDirectories(SteamFolder);
                if (dirs.Length > 0)
                {
                    SteamFolder = Directory.GetDirectories(SteamFolder)[0];
                    foreach (string file in Directory.EnumerateFiles(SteamFolder))
                    {
                        Match n = Regex.Match(file, @"slot(\d+)_save(\d+).ff8");

                        if (n.Success && n.Groups.Count > 0)
                        {
                            FileList[int.Parse(n.Groups[1].Value) - 1, int.Parse(n.Groups[2].Value) - 1] = read(file);
                        }
                    }
                }
            }
            else if (Directory.Exists(CD2000Folder))
            {
                string[] files = Directory.GetFiles(CD2000Folder, "*", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    foreach (string file in files)
                    {
                        Match n = Regex.Match(file, @"Slot(\d+)[\\/]save(\d+)");

                        if (n.Success && n.Groups.Count > 0)
                        {
                            FileList[int.Parse(n.Groups[1].Value) - 1, int.Parse(n.Groups[2].Value) - 1] = read(file);
                        }
                    }
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
                byte[] tmp = br.ReadBytes((int)fs.Length - 4);
                decmp = LZSS.DecompressAllNew(tmp);
            }
            //using (FileStream fs = File.Create(Path.Combine(@"d:\", Path.GetFileName(file))))
            //using (BinaryWriter bw = new BinaryWriter(fs))
            //{
            //    bw.Write(decmp);
            //}
            using (MemoryStream ms = new MemoryStream(decmp))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(0x184, SeekOrigin.Begin);
                Data d = new Data();
                d.Read(br);

                return d;
            }
        }
    }
}