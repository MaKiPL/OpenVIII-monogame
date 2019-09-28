using System;
using System.IO;
using System.Text.RegularExpressions;

namespace OpenVIII
{
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
    /// LordUrQuan helped get info on CD2000 version.
    /// </remarks>
    public static partial class Saves
    {
        private const int SteamOffset = 0x184;

        /// <summary>
        /// Documents\Square Enix\FINAL FANTASY VIII Steam\user_#######
        /// </summary>
        public static string Steam2013Folder { get; private set; }
        /// <summary>
        /// FF8DIR\Saves
        /// </summary>
        public static string CD2000Folder { get; private set; }
        /// <summary>
        /// Documents\My Games\FINAL FANTASY VIII Remastered\Steam\#################\game_data\user\saves
        /// </summary>
        public static string Steam2019Folder { get; private set; }

        public static Data[,] FileList { get; private set; }

        public static void Init()
        {
            FileList = new Data[2, 30];
            CD2000Folder = Path.Combine(Memory.FF8DIR, "Save");
            Steam2013Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Square Enix", "FINAL FANTASY VIII Steam");
            //C:\Users\pcvii\OneDrive\Documents\My Games\FINAL FANTASY VIII Remastered\Steam\76561198027029474\game_data\user\saves
            Steam2019Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY VIII Remastered","Steam");

            if (Directory.Exists(Steam2013Folder))
            {
                string[] dirs = Directory.GetDirectories(Steam2013Folder);
                if (dirs.Length > 0)
                {
                    string[] SteamFolders = Directory.GetDirectories(Steam2013Folder);
                    if (SteamFolders.Length > 0)
                    {
                        Steam2013Folder = SteamFolders[0];
                        GetFiles(Steam2013Folder, @"slot(\d+)_save(\d+).ff8");
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
                        Match n = Regex.Match(file, @"Slot(\d+)[\\/]save(\d+)", RegexOptions.IgnoreCase);

                        if (n.Success && n.Groups.Count > 0)
                        {
                            FileList[int.Parse(n.Groups[1].Value) - 1, int.Parse(n.Groups[2].Value) - 1] = read(file);
                        }
                    }
                }
            }
            else if (Directory.Exists(Steam2019Folder))
            {
                string[] dirs = Directory.GetDirectories(Steam2019Folder);

                if (dirs.Length > 0)
                {
                    string[] SteamFolders = Directory.GetDirectories(Steam2019Folder);
                    if (SteamFolders.Length > 0)
                    {
                        Steam2019Folder = Path.Combine(SteamFolders[0], "game_data", "user", "saves");
                        GetFiles(Steam2019Folder, @"slot(\d+)_save(\d+).ff8");
                    }
                }
            }
        }

        private static void GetFiles(string dir,string regex)
        {
            foreach (string file in Directory.EnumerateFiles(dir))
            {
                Match n = Regex.Match(file, regex, RegexOptions.IgnoreCase);

                if (n.Success && n.Groups.Count > 0)
                {
                    FileList[int.Parse(n.Groups[1].Value) - 1, int.Parse(n.Groups[2].Value) - 1] = read(file);
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
                byte[] tmp = br.ReadBytes((int)fs.Length - sizeof(uint));
                decmp = LZSS.DecompressAllNew(tmp);
            }

            using (MemoryStream ms = new MemoryStream(decmp))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(SteamOffset, SeekOrigin.Begin);
                Data d = new Data();
                d.Read(br);
                return d;
            }
        }
    }
}