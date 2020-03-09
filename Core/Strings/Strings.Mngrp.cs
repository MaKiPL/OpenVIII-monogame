using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    public partial class Strings
    {
        #region Classes

        public class MenuGroup : StringsBase
        {
            #region Fields

            private readonly IReadOnlyDictionary<int, uint> _binMsg;
            private readonly IReadOnlyDictionary<int, IReadOnlyList<int>> _complexStr;
            private readonly uint[] _stringsLoc;
            private readonly uint[] _stringsPadLoc;

            #endregion Fields

            #region Constructors

            public MenuGroup()
            {

                //string contain padding values at start of file
                //then location data before strings
                _stringsPadLoc = new[] { (uint)SectionID.Tkmnmes1, (uint)SectionID.Tkmnmes2, (uint)SectionID.Tkmnmes3 };
                //only location data before strings
                _stringsLoc = new uint[] { 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
                    55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
                    71, 72, 73, 81, 82, 83, 84, 85, 86, 87, 88, 116};
                //complex str has locations in first file,
                //and they have 8 bytes of stuff at the start of each entry, 6 bytes UNK and ushort length?
                //also can have multiple null ending strings per entry.
                _complexStr = new Dictionary<int, IReadOnlyList<int>> { { 74, new List<int> { 75, 76, 77, 78, 79, 80 } } };
                //these files come in pairs. the bin has string offsets and 6 bytes of other data
                //msg is where the strings are.
                _binMsg = new Dictionary<int, uint>
                    {{106,111},{107,112},{108,113},{109,114},{110,115}};
            }

            public static MenuGroup Load() => Load<MenuGroup>();

            protected override void DefaultValues() => SetValues(Memory.Archives.A_MENU, "mngrp.bin", "mngrphd.bin");

            #endregion Constructors

            #region Enums

            private enum SectionID
            {
                Tkmnmes1,
                Tkmnmes2,
                Tkmnmes3,
            }

            #endregion Enums

            #region Methods

            protected void GetFileLocations()
            {
                ArchiveBase aw = ArchiveWorker.Load(Archive, true);
                byte[] buffer = aw.GetBinaryFile(FileNames[1], true);
                if (buffer == null) return;
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                {
                    GetFileLocations(br);
                }
            }

            protected virtual void GetFileLocations(BinaryReader br)
            {
                while (br.BaseStream.Position+8 < br.BaseStream.Length)
                {
                    (uint seek, uint length) = ((br.ReadUInt32(), br.ReadUInt32()));
                    if (seek == 0xFFFFFFFF || length == 0x00000000) continue;
                    seek--;
                    StringFiles.SubPositions.Add(Loc.CreateInstance(seek,length));
                }
            }

            protected override void LoadArchiveFiles()
            {
                StringFiles = new StringFile(118);
                GetFileLocations();
                ArchiveBase aw = ArchiveWorker.Load(Archive, true);
                byte[] buffer = aw.GetBinaryFile(FileNames[0], true);
                if (buffer == null) return;
                using (BinaryReader br = new BinaryReader(new MemoryStream(buffer)))
                {
                    for (int key = 0; key < StringFiles.SubPositions.Count; key++)
                    {
                        bool pad = (Array.IndexOf(_stringsPadLoc, key) >= 0);
                        if (pad || Array.IndexOf(_stringsLoc, key) >= 0)
                            Get_Strings_Offsets(br, FileNames[0], key, pad);
                        else if (_binMsg.ContainsKey(key))
                        {
                            Get_Strings_BinMSG(br, FileNames[0], key, StringFiles.SubPositions[(int)_binMsg[key]].Seek, 1, 6);
                        }
                        else if (_complexStr.ContainsKey(key))
                        {
                            Get_Strings_ComplexStr(br, FileNames[0], key, _complexStr[key]);
                        }
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}