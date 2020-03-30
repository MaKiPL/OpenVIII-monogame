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

        public sealed class MenuGroup : StringsBase
        {
            #region Fields

            /// <summary>
            /// these files come in pairs. the bin has string offsets and 6 bytes of other data
            /// msg is where the strings are.
            /// </summary>
            private static readonly IReadOnlyDictionary<int, uint> BinMsg = new Dictionary<int, uint>
            {{106,111},{107,112},{108,113},{109,114},{110,115}};

            /// <summary>
            /// complex str has locations in first file, and they have 8 bytes of stuff at the start
            /// of each entry, 6 bytes UNK and ushort length? also can have multiple null ending
            /// strings per entry.
            /// </summary>
            private static readonly IReadOnlyDictionary<int, IReadOnlyList<int>> ComplexStr = new Dictionary<int, IReadOnlyList<int>> { { 74, new List<int> { 75, 76, 77, 78, 79, 80 } } };

            /// <summary>
            /// only location data before strings
            /// </summary>
            private static readonly int[] StringsLoc = { 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
                55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
                71, 72, 73, 81, 82, 83, 84, 85, 86, 87, 88, 116};

            /// <summary>
            /// string contain padding values at start of file then location data before strings
            /// </summary>
            private static readonly int[] StringsPadLoc = { (int)SectionID.Tkmnmes1, (int)SectionID.Tkmnmes2, (int)SectionID.Tkmnmes3 };

            #endregion Fields

            #region Enums

            private enum SectionID
            {
                Tkmnmes1,
                Tkmnmes2,
                Tkmnmes3,
            }

            #endregion Enums

            #region Methods

            public static MenuGroup Load() => Load<MenuGroup>();

            protected override void DefaultValues() => SetValues(Memory.Archives.A_MENU, "mngrp.bin", "mngrphd.bin");

            protected override void LoadArchiveFiles()
            {
                StringFiles = new StringFile(118);
                GetFileLocations();
                var aw = ArchiveWorker.Load(Archive, true);
                var buffer = aw.GetBinaryFile(FileNames[0], true);
                if (buffer == null) return;
                using (var br = new BinaryReader(new MemoryStream(buffer)))
                {
                    for (var key = 0; key < StringFiles.SubPositions.Count; key++)
                    {
                        var pad = (Array.IndexOf(StringsPadLoc, key) >= 0);
                        if (pad || Array.IndexOf(StringsLoc, key) >= 0)
                            Get_Strings_Offsets(br, FileNames[0], key, pad);
                        else if (BinMsg.ContainsKey(key))
                        {
                            Get_Strings_BinMSG(br, FileNames[0], key, StringFiles.SubPositions[(int)BinMsg[key]].Seek, 1, 6);
                        }
                        else if (ComplexStr.ContainsKey(key))
                        {
                            Get_Strings_ComplexStr(br, FileNames[0], key, ComplexStr[key]);
                        }
                    }
                }
            }

            private void GetFileLocations()
            {
                var aw = ArchiveWorker.Load(Archive, true);
                var buffer = aw.GetBinaryFile(FileNames[1], true);
                if (buffer == null) return;
                using (var br = new BinaryReader(new MemoryStream(buffer)))
                {
                    GetFileLocations(br);
                }
            }

            private void GetFileLocations(BinaryReader br)
            {
                while (br.BaseStream.Position + 8 < br.BaseStream.Length)
                {
                    (var seek, var length) = ((br.ReadUInt32(), br.ReadUInt32()));
                    if (seek == 0xFFFFFFFF || length == 0x00000000) continue;
                    seek--;
                    StringFiles.SubPositions.Add(Loc.CreateInstance(seek, length));
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}