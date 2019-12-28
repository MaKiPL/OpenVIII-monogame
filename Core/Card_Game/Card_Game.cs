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
                ReadSymbolsNumbers(0, br);
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

            using (Texture2D cardback = cardtim.GetTexture(55))
            {
                cardtim.ForceSetClutColors(128);
                cardtim.ForceSetClutCount(112);
                int rows = 4;
                int size = cardback.Height / rows;
                int cols = cardback.Width / size;

                //using (Texture2D combined = new Texture2D(Memory.graphics.GraphicsDevice, cardback.Width, cardback.Height))
                //{
                Color[] data = new Color[size * size];
                Rectangle src = new Rectangle(new Point(size * (cols - 2), size * (rows - 1)), new Point(size));
                Rectangle dst;
                //cardback.GetData(0, src, data, 0, data.Length);
                //combined.SetData(0, src, data, 0, data.Length);
                int row = 0;
                int acol = 0;
                List<Point> rc = new List<Point>(110);
                int page = 0;
                int pages = cols / 2;
                List<TextureHandler> th = new List<TextureHandler>(pages);
                ushort i = 0;
                string filename = $"cards";
                while (i < 110 && page < pages)
                {
                    Texture2D pagetex = new Texture2D(Memory.graphics.GraphicsDevice, cardback.Height, cardback.Height);
                    for (int p = 0; p < 8 && i < 110; p++)
                    {
                        Texture2D cardface = cardtim.GetTexture(i);
                        int coloff = i % 2;
                        bool even = coloff == 0;
                        if (row >= rows)
                        {
                            row = 0;
                            acol += 2;
                        }
                        int col = acol + coloff;
                        src = new Rectangle(new Point(size * (col), size * (row)), new Point(size));
                        dst = new Rectangle(new Point(size * (coloff), size * (row)), new Point(size));
                        cardface.GetData(0, src, data, 0, data.Length);
                        //combined.SetData(0, src, data, 0, data.Length);
                        pagetex.SetData(0, dst, data, 0, data.Length);
                        rc.Add(new Point(col, row));
                        if (!even) row++;
                        i++;
                    }
                    if (page == pages - 1)
                    {
                        src = new Rectangle(new Point(size * (cols - 2), size * (rows - 1)), new Point(size));
                        dst = new Rectangle(new Point(0, size * (rows - 1)), new Point(size));
                        cardback.GetData(0, src, data, 0, data.Length);
                        pagetex.SetData(0, dst, data, 0, data.Length);
                    }
                    if (Memory.EnableDumpingData)
                        using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), $"{filename}_{page}.png"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                            pagetex.SaveAsPng(fs, cardback.Height, cardback.Height);
                    th.Add(TextureHandler.Create($"{filename}_{page}.png", new Texture2DWrapper(pagetex)));
                    page++;
                }
                CardFaces = th.ToArray();
                //    if (Memory.EnableDumpingData)
                //        using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), $"{filename}.combined.png"), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                //            combined.SaveAsPng(fs, cardback.Width, cardback.Height);
                //}
            }
        }

        private void ReadSymbolsNumbers(int id, BinaryReader br)
        {
            //3 pages, 256x256; inside () is palette id +1.
            //page 1 = 5 rows. first 3 rows are 16x16 grid, last 2 rows are a 24x24 grid
            //          row 1 has 11 hex numbers: 0-A (1)
            //          row 2 4 of 4 frame animations: fire(4), ice(7), lightning(10), earth(13)
            //          row 3 4 of 4 frame animations: poison(16), wind(19), water(22), holy(25)
            //          row 4 has 9 numbers: 0-9 (28)
            //          row 5 has 2 items: +1, -1 (31 or 34)
            //page 2 = 3 rows of 256x48: You Win!(2), You Lose...(5), Draw(8)
            //page 3 = 3 rows of 256x64: Same!(9), Plus!(6), Combo!(9)


            TIM2 temp = new TIM2(br, EXE_Offsets.TIM[id]);
            temp.ForceSetClutColors(16);
            temp.ForceSetClutCount(48);
            string filename = $"ff8exe{id.ToString("D2")}";
            //if (Memory.EnableDumpingData)
                Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });
            SymbolsNumbers = new TextureHandler[temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount; i++)
            {
                SymbolsNumbers[i] = TextureHandler.Create(filename, temp, i);
                Memory.MainThreadOnlyActions.Enqueue(SymbolsNumbers[i].Save);
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