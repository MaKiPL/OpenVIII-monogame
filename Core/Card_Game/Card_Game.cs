using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

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
                //ReadTIM(2, br, out CardFaces, );
                Memory.MainThreadOnlyActions.Enqueue(() =>
                {
                    using (BinaryReader br2 = new BinaryReader(File.OpenRead(EXE_Offsets.FileName)))
                        ReadCardFaces(2, br2);
                });
                ReadTIM(50, br, out CardGameBG);
                ReadTIM(51, br, out CardOtherBG);
            }
        }

        private void ReadCardFaces(int id, BinaryReader br)
        {
            TIM2 cardtim = new TIM2(br, EXE_Offsets.TIM[id]);
            Texture2D cardback = cardtim.GetTexture(55);
            cardtim.ForceSetClutColors(128);
            cardtim.ForceSetClutCount(112);
            int rows = 4;
            int size = cardback.Height / rows;
            int cols = cardback.Width / size;

            using (Texture2D combined = new Texture2D(Memory.graphics.GraphicsDevice, cardback.Width, cardback.Height))
            {
                Color[] data = new Color[size * size];
                Rectangle src = new Rectangle(new Point(size * (cols - 2), size * (rows - 1)), new Point(size));
                cardback.GetData(0, src, data, 0, data.Length);
                combined.SetData(0, src, data, 0, data.Length);
                int row = 0;
                int acol = 0;
                List<Point> rc = new List<Point>(110);
                for (ushort i = 0; i < 110; i++)
                {
                    Texture2D cardface = cardtim.GetTexture(i);
                    int coloff = i % 2;
                    bool even = coloff == 0;
                    if (row >= rows)
                    {
                        row = 0;
                        acol+=2;
                    }
                    int col = acol + coloff;
                    src = new Rectangle(new Point(size * (col), size * (row)), new Point(size));
                    cardface.GetData(0, src, data, 0, data.Length);
                    combined.SetData(0, src, data, 0, data.Length);
                    rc.Add(new Point(col, row));
                    if (!even) row++;
                }
                string filename = $"ff8exe{id.ToString("D2")}";
                if (Memory.EnableDumpingData)
                    using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), $"{filename}.combined.png"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                        combined.SaveAsPng(fs, cardback.Width, cardback.Height);

            }
        }

        private void ReadTIM(int id, BinaryReader br, out TextureHandler[] tex, ushort ForceSetClutColors = 0, ushort ForceSetClutCount = 0)
        {
            TIM2 temp = new TIM2(br, EXE_Offsets.TIM[id]);
            if (ForceSetClutColors > 0)
                temp.ForceSetClutColors(ForceSetClutColors);
            if (ForceSetClutCount > 0)
                temp.ForceSetClutCount(ForceSetClutCount);
            string filename = $"ff8exe{id.ToString("D2")}";
            if (Memory.EnableDumpingData)
                Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });
            tex = new TextureHandler[temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount; i++)
            {
                tex[i] = TextureHandler.Create(filename, temp, i);
            }
        }
    }
}