using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public class EXE_Offsets
    {
        public static readonly Dictionary<int, uint[]> STRINGS = new Dictionary<int, uint[]> {
            { 2013, new uint[]
                {
                    //Start with Count
                    0x875074, // Card Names, Uint16 Count, Uint16[Count] Offsets, FF8String[Count] Null Ending.
                    0x875524, // Card Names2, Uint16 Count, Uint16[Count] Offsets, FF8String[Count] Null Ending. Ignore null Offsets.
                    0x874B58, // Card Text, Uint16 Count, Uint16[Count] Offsets, FF8String[Count] Null Ending.
                    //There is no Count at start of section only offsets
                    0x7921E4, // Text related to discs and draw points, Count = 9, Uint32[Count] Offsets, FF8String[Count] Null Ending.
                    0x14838D4 // Scan Text, Count = 160, Uint16[Count] Offset from start of first string. FF8String[Count] Null Ending.
                }
            }
        };

        public static readonly Dictionary<int, uint[]> TIM = new Dictionary<int, uint[]> {
            { 2000, new uint[]
                {
                // The ones commented out weren't real tim files. But were close enough to not set off exceptions.
                    //4 bpp
                    0x795A78, // Card game symbols and numbers.
                    0x7A8698, // Random Letters, Numbers, Symbols // 2 palettes differnt characters depending on palette

                    //8 bpp
                    0x7A8AF8, // Card faces

                    0x944E8C, // Magic effect
                    0xA95AE0, // Cactar?

                    0xACC40C, // Magic effect
                    0xAF4C30, // Magic effect // failed to load? timviewer says it's valid 11488304
                    0xB1E920, // Magic effect
                    0xB47340, // Magic effect
                    0xB57D60, // Magic effect
                    0xB78384, // Magic effect // failed to load? timviewer says it's valid 12026756
                    0xBAD4A8, // magic effect // failed to load? timviewer says it's valid 12244136
                    0xC05E64, // magic effect
                    0xF76698, // Moomba
                    0xF7A8B8, // magic effect
                    0xF9B8D8, // magic effect
                    0xFBBBEC, // magic effect
                    0xFED20C, // magic effect
                    0x100016C, // magic effect
                    0x102938C, // magic effect
                    0x103CC90, // magic effect
                    0x1070DC4, // magic effect
                    0x10AF700, // Angelo
                    0x10B7B20, // magic effect
                    0x10D8F58, // magic effect
                    0x10FA790, // magic effect
                    0x124A678, // monster skin or reaper?
                    0x1252A98, // bat wings
                    0x125BCCC, // chocobo
                    0x12704DC, // chocobo dup
                    0x1279714, // chocobo dup
                    0x1288434, // chocobo dup
                    0x12ADE90, // Angelo dup
                    0x12B8990, // Angelo dup
                    0x12C12C8, // Angelo dup
                    0x12CF8B0, // Angelo dup
                    0x12F3000, // Angelo dup
                    0x12FB420, // umm?
                    0x12FDF78, // Angelo dup
                    0x130CDCC, // Angelo dup
                    0x13243F8, // Angel wings
                    0x1338C08, // Cherub
                    0x135AC20, // magic effect
                    0x1378F3C, // monster skin or reaper? dup
                    0x138135C, // bat wings dup
                    0x1408FB0, // monster skin or reaper? dup
                    0x14113D0, // bat wings dup
                    0x141CFAC, // monster skin or reaper? dup
                    0x14253CC, // bat wings dup
                    0x14295EC, // spell effect

                    //16 bbp
                    0x81F818, // Card game board
                    0x849B2C, // Card loading background?
                }
            },
            { 2013, new uint[]
                {
                // The ones commented out weren't real tim files. But were close enough to not set off exceptions.
                    //4 bpp
                    0x796A90, // Card game symbols and numbers.
                    0x7A96B0, // Random Letters, Numbers, Symbols

                    //8 bpp
                    0x7A9B10, // Card faces
                    0x945EAC, // Magic effect
                    0xA96B00, // Cactar?
                    0xACC40C, // Magic effect
                    0xAF4C30, // Magic effect // failed to load? timviewer says it's valid 11488304
                    0xB1E920, // Magic effect
                    0xB47340, // Magic effect
                    0xB57D60, // Magic effect
                    0xB78384, // Magic effect // failed to load? timviewer says it's valid 12026756
                    0xBAD4A8, // magic effect // failed to load? timviewer says it's valid 12244136
                    0xC05E64, // magic effect
                    0xF76698, // Moomba
                    0xF7A8B8, // magic effect
                    0xF9B8D8, // magic effect
                    0xFBBBEC, // magic effect
                    0xFED20C, // magic effect
                    0x100016C, // magic effect
                    0x102938C, // magic effect
                    0x103CC90, // magic effect
                    0x1070DC4, // magic effect
                    0x10AF700, // Angelo
                    0x10B7B20, // magic effect
                    0x10D8F58, // magic effect
                    0x10FA790, // magic effect
                    0x124A678, // monster skin or reaper?
                    0x1252A98, // bat wings
                    0x125BCCC, // chocobo
                    0x12704DC, // chocobo dup
                    0x1279714, // chocobo dup
                    0x1288434, // chocobo dup
                    0x12ADE90, // Angelo dup
                    0x12B8990, // Angelo dup
                    0x12C12C8, // Angelo dup
                    0x12CF8B0, // Angelo dup
                    0x12F3000, // Angelo dup
                    0x12FB420, // umm?
                    0x12FDF78, // Angelo dup
                    0x130CDCC, // Angelo dup
                    0x13243F8, // Angel wings
                    0x1338C08, // Cherub
                    0x135AC20, // magic effect
                    0x1378F3C, // monster skin or reaper? dup
                    0x138135C, // bat wings dup
                    0x1408FB0, // monster skin or reaper? dup
                    0x14113D0, // bat wings dup
                    0x141CFAC, // monster skin or reaper? dup
                    0x14253CC, // bat wings dup
                    0x14295EC, // spell effect

                    //16 bbp
                    0x820B30, // Card game board
                    0x84AB44, // Card loading background?
                }
            }
        };

        ///// <summary>
        ///// If error occurs the bad offsets are added to list.
        ///// Use Debugger to check what is bad.
        ///// </summary>
        //private List<uint> badoffset;
        public static Dictionary<int, string> FileName => new Dictionary<int, string>
        {
            { 2000, Path.Combine(Memory.FF8DIR, "FF8.exe") },
            { 2013, Path.Combine(Memory.FF8DIR, "FF8_EN.exe") },
        };

        public EXE_Offsets()
        {
            //badoffset = new List<uint>();
            ////try
            ////{
            //    using (BinaryReader br = new BinaryReader(File.OpenRead(FileName)))
            //    {
            //        foreach (uint t in TIM)
            //        {
            //            try
            //            {
            //                TIM2 timdata = new TIM2(br, t);
            //                Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Timages"));
            //                timdata.Save(Path.Combine(Path.GetTempPath(), "Timages", string.Format("0x{0:X}.tim", t)));
            //            }
            //            catch (InvalidDataException e)
            //            {
            //                if (e.Message == "Invalid TIM File")
            //                {
            //                    badoffset.Add(t);
            //                }
            //                else e.Rethrow();
            //            }
            //        }
            //    }
            ////}
            ////catch (IOException e)
            ////{
            ////    //search is locking down the file woot.
            ////}
        }
    }
}