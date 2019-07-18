using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.Card_Game
{
    class EXE_Offsets
    {
        public readonly int[] tim =
        {
            //cards
            0x7A9B10, 
            //UNK
            0x796A90,
            0x7A96B0,
            0x875C09,
            0x875C2D,
            0xC892D3,
            0xC893CF,
            0xC8EF37,
            0xC8F067,
            0xC8F197,
            0xC8F2EF,
            0xC8F447,
            0xC8F59F,
            0xC8F707,
            0xC8F86F,
            0xC8F9D7,
            0xD87D8B,
            0xDC8E77,
            0xE00CE7,
            0xE9670F,
            0xE96A4B,
            0xEA9123,
            0xF0C9D7,
            0x102F4BF,
            0x102F617,
            0x1069E6B,
            0x106A26F,
            0x106A3E7,
            0x12F2D17,
            0x12F2E27,
            0x130C387,
            0x103C73F,
            0x131A5C9,
            0x131A60D,
        };
    }
    class Card_Game
    {
        const int exe_Offset = 0x7A9B10;
        string fileName => Path.Combine(Memory.FF8DIR,"FF8_EN.exe");
        public Card_Game()
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(fileName)))
            {
                TIM2 tim = new TIM2(br,exe_Offset);

            }
        }
    }
}
