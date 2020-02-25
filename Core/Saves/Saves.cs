using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// wanna use kaitai. LordUrQuan helped get info on CD2000 version.
    /// </remarks>
    public static partial class Saves
    {
        #region Fields

        private const int GamesPerSlot = 30;
        private const int Slots = 2;
        private const int SteamOffset = 0x184;

        #endregion Fields

        #region Properties

        /// <summary>
        /// FF8DIR\Saves
        /// </summary>
        public static string CD2000Folder { get; private set; }

        public static Data[,] FileList { get; private set; }

        /// <summary>
        /// Documents\Square Enix\FINAL FANTASY VIII Steam\user_#######
        /// </summary>
        public static string Steam2013Folder { get; private set; }

        /// <summary>
        /// Documents\My Games\FINAL FANTASY VIII Remastered\Steam\#################\game_data\user\saves
        /// </summary>
        public static string Steam2019Folder { get; private set; }

        #endregion Properties

        #region Methods

        public static void Init()
        {
            Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(Init)}");
            FileList = new Data[Slots, GamesPerSlot];
            CD2000Folder = Path.Combine(Memory.FF8DIR, "Save");
            Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(CD2000Folder)} :: {CD2000Folder}");
            Steam2013Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Square Enix", "FINAL FANTASY VIII Steam");
            Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(Steam2013Folder)} :: {Steam2013Folder}");
            Steam2019Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY VIII Remastered", "Steam");
            Memory.Log.WriteLine($"{nameof(Saves)} :: {nameof(Steam2019Folder)} :: {Steam2019Folder}");

            if (Directory.Exists(Steam2013Folder))
            {
                string[] dirs = Directory.GetDirectories(Steam2013Folder);
                if (dirs.Length > 0)
                {
                    string[] SteamFolders = Directory.GetDirectories(Steam2013Folder);
                    if (SteamFolders.Length > 0)
                    {
                        Steam2013Folder = SteamFolders[0];
                        GetFiles(Steam2013Folder, @"slot(\d+)_save(\d+).ff8$");
                    }
                }
            }
            else if (Directory.Exists(CD2000Folder))
            {
                ProcessFiles(Directory.GetFiles(CD2000Folder, "*", SearchOption.AllDirectories), @"Slot(\d+)[\\/]save(\d+)$");
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
                        GetFiles(Steam2019Folder, @"slot(\d+)_save(\d+).ff8$");
                    }
                }
            }
        }

        private static void GetFiles(string dir, string regex) => ProcessFiles(Directory.EnumerateFiles(dir), regex);

        private static void ProcessFiles(IEnumerable<string> files, string pattern)
        {
            //List<Action> actions = new List<Action>();
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            IEnumerable<Action> actions = files.Select(x => new { file = x, match = regex.Match(x) })
                .Where(x => x.match.Success && x.match.Groups.Count >= 3)
                .Select(x =>
                {
                    if (!int.TryParse(x.match.Groups[1].Value, out int slot) || !int.TryParse(x.match.Groups[2].Value, out int game))
                    {
                        slot = -1;
                        game = -1;
                    }
                    return new { x.file, slot = slot - 1, game = game - 1 };
                })
                .Where(x => x.slot < Slots && x.game < GamesPerSlot && x.game >= 0 && x.slot >= 0)
                .Select(x => new Action(() => { Read(x.file, out FileList[x.slot, x.game]); }));
            Memory.ProcessActions(actions.ToArray());
        }

        private static void Read(string file, out Data d)
        {
            d = null;
            int size = 0;
            Memory.Log.WriteLine($"{nameof(Saves)}::{nameof(Read)} Extracting: {file}");
            byte[] decmp = null;
            MemoryStream ms = null;
            FileStream fs = null;

            // fs is disposed by binaryreader.
            using (BinaryReader br = new BinaryReader(
                fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                if (fs.Length > 5)
                {
                    size = br.ReadInt32();
                    byte[] tmp = br.ReadBytes((int)fs.Length - sizeof(uint));
                    decmp = LZSS.DecompressAllNew(tmp);
                }
                fs = null;
            }

            if (decmp == null || decmp.Length < size)
            {
                Memory.Log.WriteLine($"{nameof(Saves)}::{nameof(Read)} Invalid file: {file}");
            }
            else
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(decmp)))
                {
                    ms.Seek(SteamOffset, SeekOrigin.Begin);
                    d = new Data();
                    d.Read(br);
                    ms = null;
                }
        }

        #endregion Methods
    }
}