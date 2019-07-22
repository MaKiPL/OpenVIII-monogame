using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    internal class EXE_Offsets
    {
        public readonly uint[] tim =
        {// The ones commented out weren't real tim files. But were close enough to not set off exceptions.
            //4 bpp
            0x796A90, // Card game symbols and numbers.
            0x7A96B0, // Random Letters
            
            //8 bpp
            0x7A9B10, // cards
            0x945EAC,
            0xA96B00,
            0xACC40C,
            //0xAF4C30,
            0xB1E920,
            0xB47340,
            0xB57D60,
            //0xB78384,
            //0xBAD4A8,
            0xC05E64,
            0xF76698,
            0xF7A8B8,
            0xF9B8D8,
            0xFBBBEC,
            0xFED20C,
            0x100016C,
            0x102938C,
            0x103CC90,
            0x1070DC4,
            0x10AF700,
            0x10B7B20,
            0x10D8F58,
            0x10FA790,
            //0x12146E8,
            //0x1214908,
            //0x1214B28,
            //0x1214D48,
            0x124A678,
            0x1252A98,
            0x125BCCC,
            0x12704DC,
            0x1279714,
            0x1288434,
            0x12ADE90,
            0x12B8990,
            0x12C12C8,
            0x12CF8B0,
            0x12F3000,
            0x12FB420,
            0x12FDF78,
            0x130CDCC,
            0x13243F8,
            0x1338C08,
            0x135AC20,
            0x1378F3C,
            0x138135C,
            0x1408FB0,
            0x14113D0,
            0x141CFAC,
            0x14253CC,
            0x14295EC,

            //16 bbp
            0x820B30,
            0x84AB44,



        };
        /// <summary>
        /// If error occurs the bad offsets are added to list.
        /// Use Debugger to check what is bad.
        /// </summary>
        private List<uint> badoffset;
        public string FileName => Path.Combine(Memory.FF8DIR, "FF8_EN.exe");

        public EXE_Offsets()
        {
            badoffset = new List<uint>();
            //try
            //{
                using (BinaryReader br = new BinaryReader(File.OpenRead(FileName)))
                {
                    foreach (uint t in tim)
                    {
                        try
                        {
                        if (0x84AB44 == t)
                        {
                        }
                            TIM2 timdata = new TIM2(br, t);
                            Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "Timages"));
                            timdata.Save(Path.Combine(Path.GetTempPath(), "Timages", string.Format("0x{0:X}.tim", t)));
                        }
                        catch (InvalidDataException e)
                        {
                            if (e.Message == "Invalid TIM File")
                            {
                                badoffset.Add(t);
                            }
                            else e.Rethrow();
                        }
                    }
                }
            //}
            //catch (IOException e)
            //{
            //    //search is locking down the file woot.
            //}
        }
    }

    public class Card_Game
    {
        private const int exe_Offset = 0x7A9B10;

        private string fileName => Path.Combine(Memory.FF8DIR, "FF8_EN.exe");

        public Card_Game()
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(fileName)))
            {
                TIM2 tim = new TIM2(br, exe_Offset);
            }
        }
    }
}