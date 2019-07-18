using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Core.Card_Game
{
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
