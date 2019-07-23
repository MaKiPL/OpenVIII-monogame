using System.IO;

namespace OpenVIII
{
    public class Card_Game
    {
        public Card_Game()
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(EXE_Offsets.FileName)))
            {
                TIM2 SymbolsNumbers = new TIM2(br, EXE_Offsets.TIM[0]);
                TIM2 CardFaces = new TIM2(br, EXE_Offsets.TIM[2]);
                TIM2 CardGameBG = new TIM2(br, EXE_Offsets.TIM[50]);
                TIM2 CardOtherBG = new TIM2(br, EXE_Offsets.TIM[51]);
            }
        }
    }
}