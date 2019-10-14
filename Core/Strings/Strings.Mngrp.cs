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

        public class Mngrp : StringsBase
        {
            #region Fields

            private Dictionary<uint, uint> BinMSG;
            private Dictionary<uint, List<uint>> ComplexStr;
            private uint[] StringsLoc;
            private uint[] StringsPadLoc;

            #endregion Fields

            #region Constructors

            public Mngrp() : base(Memory.Archives.A_MENU, "mngrp.bin", "mngrphd.bin")
            {
            }

            #endregion Constructors

            #region Enums

            private enum SectionID
            {
                tkmnmes1,
                tkmnmes2,
                tkmnmes3,
            }

            #endregion Enums

            #region Methods

            protected void GetFileLocations()
            {
                ArchiveWorker aw = new ArchiveWorker(Archive, true);
                MemoryStream ms = null;
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(aw.GetBinaryFile(Filenames[1], true))))
                {
                    GetFileLocations(br);
                    ms = null;
                }
            }

            protected override void GetFileLocations(BinaryReader br)
            {
                while (br.BaseStream.Position < br.BaseStream.Length)
                {
                    Loc loc = new Loc() { seek = br.ReadUInt32(), length = br.ReadUInt32() };
                    if (loc.seek != 0xFFFFFFFF && loc.length != 0x00000000)
                    {
                        loc.seek--;
                        Files.subPositions.Add(loc);
                    }
                }
            }

            protected override void Init()
            {
                Files = new StringFile(118);
                GetFileLocations();
                ArchiveWorker aw = new ArchiveWorker(Archive, true);
                MemoryStream ms = null;
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(aw.GetBinaryFile(Filenames[0], true))))
                {
                    //string contain padding values at start of file
                    //then location data before strings
                    StringsPadLoc = new uint[] { (uint)SectionID.tkmnmes1, (uint)SectionID.tkmnmes2, (uint)SectionID.tkmnmes3 };
                    //only location data before strings
                    StringsLoc = new uint[] { 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54,
                            55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
                            71, 72, 73, 81, 82, 83, 84, 85, 86, 87, 88, 116};
                    //complexstr has locations in first file,
                    //and they have 8 bytes of stuff at the start of each entry, 6 bytes UNK and ushort length?
                    //also can have multiple null ending strings per entry.
                    ComplexStr = new Dictionary<uint, List<uint>> { { 74, new List<uint> { 75, 76, 77, 78, 79, 80 } } };
                    //these files come in pairs. the bin has string offsets and 6 bytes of other data
                    //msg is where the strings are.
                    BinMSG = new Dictionary<uint, uint>
                            {{106,111},{107,112},{108,113},{109,114},{110,115}};

                    for (uint key = 0; key < Files.subPositions.Count; key++)
                    {
                        Loc fpos = Files.subPositions[(int)key];
                        bool pad = (Array.IndexOf(StringsPadLoc, key) >= 0);
                        if (pad || Array.IndexOf(StringsLoc, key) >= 0)
                            Get_Strings_Offsets(br, Filenames[0], key, pad);
                        else if (BinMSG.ContainsKey(key))
                        {
                            Get_Strings_BinMSG(br, Filenames[0], key, Files.subPositions[(int)BinMSG[key]].seek, 1, 6);
                        }
                        else if (ComplexStr.ContainsKey(key))
                        {
                            Get_Strings_ComplexStr(br, Filenames[0], key, ComplexStr[key]);
                        }
                    }
                    ms = null;
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}