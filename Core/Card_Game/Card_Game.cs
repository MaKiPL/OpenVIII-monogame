using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public class Card_Game
    {
        private TextureHandler[] SymbolsNumbers;
        private TextureHandler[] CardFaces;
        private TextureHandler[] CardGameBG;
        private TextureHandler[] CardOtherBG;

        public Card_Game()
        {
            using (BinaryReader br = new BinaryReader(File.OpenRead(EXE_Offsets.FileName)))
            {
                ReadTIM(0, br, out SymbolsNumbers);
                ReadTIM(2, br, out CardFaces);
                ReadTIM(50, br, out CardGameBG);
                ReadTIM(51, br, out CardOtherBG);
            }
        }

        private void ReadTIM(int id, BinaryReader br,out TextureHandler[] tex)
        {
            var temp = new TIM2(br, EXE_Offsets.TIM[id]);
            tex = new TextureHandler[temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount; i++)
                tex[i] = TextureHandler.Create($"ff8exe{id.ToString("D2")}", temp, i);
        }
    }
}