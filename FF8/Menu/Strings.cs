using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8
{
    /// <summary>
    /// Loads strings from FF8 files
    /// </summary>
    internal class Strings
    {
        private readonly string[] filenames = new string[] { "mngrp.bin", "mngrphd.bin" };
        private List<Loc> mngrp_SubPositions;
        private Dictionary<uint, List<uint>> sPositions;
        private uint[] StringsPadLoc;
        private uint[] StringsLoc;
        private Dictionary<uint, uint> BinMSG;
        private string ArchiveString;
        private ArchiveWorker aw;

        public Strings() => init();

        public enum FileID : uint
        {
            MNGRP = 0,
            MNGRP_MAP = 1,
        }

        public enum SubID : uint
        {
            tkmnmes1 = 0,
            tkmnmes2 = 1,
            tkmnmes3 = 2,
        }

        private void init()
        {
            ArchiveString = Memory.Archives.A_MENU;
            aw = new ArchiveWorker(ArchiveString);
            mngrp_init();
        }

        private void mngrp_GetFileLocations()
        {
            mngrp_SubPositions = new List<Loc>(118);
            using (MemoryStream ms = new MemoryStream(aw.GetBinaryFile(
                aw.GetListOfFiles().First(x => x.IndexOf(filenames[(int)FileID.MNGRP_MAP], StringComparison.OrdinalIgnoreCase) >= 0))))
            using (BinaryReader br = new BinaryReader(ms))
            {
                while (ms.Position < ms.Length)
                {
                    Loc loc = new Loc() { seek = br.ReadUInt32(), length = br.ReadUInt32() };
                    if (loc.seek != 0xFFFFFFFF && loc.length != 0x00000000)
                    {
                        loc.seek--;
                        mngrp_SubPositions.Add(loc);
                    }
                }
            }
        }

        private void mngrp_init()
        {
            mngrp_GetFileLocations();
            sPositions = new Dictionary<uint, List<uint>>(mngrp_SubPositions.Count);
            using (MemoryStream ms = new MemoryStream(aw.GetBinaryFile(
                aw.GetListOfFiles().First(x => x.IndexOf(filenames[(int)FileID.MNGRP], StringComparison.OrdinalIgnoreCase) >= 0))))
            using (BinaryReader br = new BinaryReader(ms))
            {
                StringsPadLoc = new uint[] { (uint)SubID.tkmnmes1, (uint)SubID.tkmnmes2, (uint)SubID.tkmnmes3 };
                StringsLoc = new uint[] { 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73 };

                BinMSG = new Dictionary<uint, uint>
                {{106,111},{107,112},{108,113},{109,114},{110,115}};

                for (uint f = 0; f < mngrp_SubPositions.Count; f++)
                {
                    Loc fpos = mngrp_SubPositions[(int)f];
                    bool pad = (Array.IndexOf(StringsPadLoc, f) >= 0);
                    if (pad || Array.IndexOf(StringsLoc, f) >= 0)
                        mngrp_get_string_offsets(br, f, pad);
                    else if (BinMSG.ContainsKey(f))
                    {
                        mngrp_get_string_BinMSG(br, f, mngrp_SubPositions[(int)BinMSG[f]].seek);
                    }
                }
            }
        }

        private void mngrp_get_string_BinMSG(BinaryReader br, uint f, uint msgPos)
        {
            Loc fpos = mngrp_SubPositions[(int)f];
            br.BaseStream.Seek(fpos.seek, SeekOrigin.Begin);
            if (sPositions.ContainsKey(f))
            {
            }
            else
            {
                ushort b = 0;
                ushort last = b;
                sPositions.Add(f, new List<uint>());
                while (br.BaseStream.Position < fpos.max)
                {
                    b = br.ReadUInt16();
                    if (last > b)
                        break;
                    else
                    {
                        sPositions[f].Add(b + msgPos);
                        br.BaseStream.Seek(6, SeekOrigin.Current);
                        last = b;
                    }
                }
            }
        }

        private byte[] readString(FileID fid, uint pos)
        {
            using (MemoryStream ms = new MemoryStream(aw.GetBinaryFile(
                aw.GetListOfFiles().First(x => x.IndexOf(filenames[(int)FileID.MNGRP], StringComparison.OrdinalIgnoreCase) >= 0))))
            using (BinaryReader br = new BinaryReader(ms))
            {
                return readString(br, fid, pos);
            }
        }

        private byte[] readString(BinaryReader br, FileID fid, uint pos)
        {
            using (MemoryStream os = new MemoryStream(50))
            {
                br.BaseStream.Seek(pos, SeekOrigin.Begin);
                byte b = 0;
                do
                {
                    b = br.ReadByte();
                    if (b != 0) os.WriteByte(b);
                }
                while (b != 0);
                if (os.Length > 0)
                    return os.ToArray();
            }
            return null;
        }

        public byte[] GetEntry(FileID fid, SubID sid, int b) => GetEntry(fid, (int)sid, b);

        public byte[] GetEntry(FileID fid, int sid, int b)
        {
            switch (fid)
            {
                case FileID.MNGRP:
                case FileID.MNGRP_MAP:
                    return readString(fid, sPositions[(uint)sid][b]);
            }

            return null;
        }

        private void mngrp_get_string_offsets(BinaryReader br, uint f, bool pad = false)
        {
            Loc fpos = mngrp_SubPositions[(int)f];
            uint[] fPaddings;
            if (pad)
                fPaddings = mngrp_read_padding(br, fpos);
            else
                fPaddings = new uint[] { fpos.seek };
            sPositions.Add(f, new List<uint>());
            foreach (uint fpad in fPaddings)
            {
                if (fpad <= 0) continue;
                br.BaseStream.Seek(fpad, SeekOrigin.Begin);
                if (br.BaseStream.Position + 4 < br.BaseStream.Length)
                {
                    int count = br.ReadUInt16();
                    for (int i = 0; i < count && br.BaseStream.Position + 2 < br.BaseStream.Length; i++)
                    {
                        uint c = br.ReadUInt16();
                        if (c == 0) continue;
                        c += fpad;
                        sPositions[f].Add(c);
                    }
                }
            }
        }

        private uint[] mngrp_read_padding(BinaryReader br, Loc fpos)
        {
            uint[] fPaddings = null;
            br.BaseStream.Seek(fpos.seek, SeekOrigin.Begin);
            fPaddings = new uint[br.ReadUInt16()];
            for (int i = 0; i < fPaddings.Length; i++)
            {
                fPaddings[i] = br.ReadUInt16();
                if (fPaddings[i] + fpos.seek >= fpos.max)
                    fPaddings[i] = 0;
                if (fPaddings[i] != 0)
                    fPaddings[i] += fpos.seek;
            }
            return fPaddings;
        }

        public void DumpMe(string path)
        {
            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))

            using (MemoryStream ms = new MemoryStream(aw.GetBinaryFile(
                aw.GetListOfFiles().First(x => x.IndexOf(filenames[(int)FileID.MNGRP], StringComparison.OrdinalIgnoreCase) >= 0))))
            using (BinaryReader br = new BinaryReader(ms))
            {
                uint last = 0;
                foreach (KeyValuePair<uint, List<uint>> s in sPositions)
                {
                    Loc fpos = mngrp_SubPositions[(int)s.Key];
                    if (s.Key == 0)
                    {
                        last = s.Key;
                        bw.Write(Encoding.UTF8.GetBytes($"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>\n<file id={s.Key} seek={fpos.seek} length={fpos.length}>\n"));
                    }
                    else
                    {
                        last = s.Key;
                        bw.Write(Encoding.UTF8.GetBytes($"</file>\n<fpos id={s.Key} seek={fpos.seek} length={fpos.length}>\n"));
                    }

                    for (int j = 0; j < s.Value.Count; j++)
                    {
                        byte[] b = Font.DecodeDirty(readString(br, FileID.MNGRP, s.Value[j]));
                        if (b != null)
                        {
                            bw.Write(Encoding.UTF8.GetBytes($"\t<string id={j} seek={s.Value[j]}>"));
                            bw.Write(b);
                            bw.Write(Encoding.UTF8.GetBytes("</string>\n"));
                        }
                    }
                }
                bw.Write(Encoding.UTF8.GetBytes($"</file>"));
            }
        }

        //private void readfile()
        //{
        //    //text is prescrabbled and is ready to draw to screen using font renderer

        // //based on what I see here some parts of menu ignore \n and some will not //example when
        // you highlight a item to refine it will only show only on one line up above. // 1 will
        // refine into 20 Thundaras //and when you select it the whole string will show. // Coral
        // Fragment: // 1 will refine // into 20 Thundaras

        // //m000.msg = 104 strings //example = Coral Fragment:\n1 will refine \ninto 20 Thundaras\0
        // //I think this one is any item that turns into magic //___ Mag-RF items? except for
        // upgrade abilities

        // //m001.msg = 145 strings //same format differnt items //example = Tent:\n4 will refine
        // into \n1 Mega-Potion\0 //I think this one is any item that turns into another item //___
        // Med-RF items? except for upgrade abilities //guessing Ammo-RF is in here too.

        // //m002.msg = 10 strings //same format differnt items //example = Fire:\n5 will refine
        // \ninto 1 Fira\0 //this one is magic tha turns into higher level magic //first 4 are Mid
        // Mag-RF //last 6 are High Mag-RF

        // //m003.msg = 12 strings //same format differnt items //example = Elixer:\n10 will refine
        // \ninto 1 Megalixir\0 //this one is Med items tha turns into higher level Med items //all
        // 12 are Med LV Up

        // //m004.msg = 110 strings //same format differnt items //example = Geezard Card:\n1 will
        // refine \ninto 5 Screws\0 //this one is converts cards into items //all 110 are Card Mod

        // //mwepon.msg = 34 strings //all strings are " " or " " kinda a odd file.

        // //pet_exp.msg = 18 strings //format: ability name\0description\0 //{0x0340} = Angelo's
        // name //example: {0x0340} Rush\0 Damage one enemy\0 //list of Angelo's attack names and descriptions

        //    //namedic.bin 32 strings
        //    //Seems to be location names.
        //    //start of file
        //    //  UIint16 Count
        //    //  UIint16[Count]Location
        //    //at each location
        //    //  Byte[Count][Bytes to null]
        //}
    }
}