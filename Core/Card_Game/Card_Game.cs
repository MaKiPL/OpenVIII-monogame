using System;
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
                ReadTIM(2, br, out CardFaces,128,112);
                ReadTIM(50, br, out CardGameBG);
                ReadTIM(51, br, out CardOtherBG);
            }
        }

        private void ReadTIM(int id, BinaryReader br,out TextureHandler[] tex, ushort ForceSetClutColors = 0, ushort ForceSetClutCount =0)
        {
            var temp = new TIM2(br, EXE_Offsets.TIM[id]);
            if (ForceSetClutColors > 0)
                temp.ForceSetClutColors(ForceSetClutColors);
            if (ForceSetClutCount > 0)
                temp.ForceSetClutCount(ForceSetClutCount);
            string filename = $"ff8exe{id.ToString("D2")}";
            Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });            
            tex = new TextureHandler[temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount; i++)
            {
                tex[i] = TextureHandler.Create(filename, temp, i);
            }
        }
    }
}