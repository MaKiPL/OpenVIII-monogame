using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    public class Card_Game
    {
        private const int year = 2013;
        private TextureHandler[] _symbolsNumbers;
        private TextureHandler[] _cardFaces;
        private TextureHandler[] _cardGameBG;
        private TextureHandler[] _cardOtherBG;

        public TextureHandler[] SymbolsNumbers { get => _symbolsNumbers; private set => _symbolsNumbers = value; }

        //private TextureHandler[] Other;
        public TextureHandler[] CardFaces { get => _cardFaces; private set => _cardFaces = value; }

        public TextureHandler[] CardGameBG { get => _cardGameBG; private set => _cardGameBG = value; }
        public TextureHandler[] CardOtherBG { get => _cardOtherBG; private set => _cardOtherBG = value; }

        public List<Entry> SymbolNumberEntries { get; private set; }

        public Card_Game()
        {
            using (BinaryReader br = new BinaryReader(FileStreamOpen()))
            {
                Memory.MainThreadOnlyActions.Enqueue(() =>
                {
                    using (BinaryReader br2 = new BinaryReader(FileStreamOpen()))
                        ReadSymbolsNumbers(0, br2);
                });
                //ReadTIM(1, br,out Other);

                Memory.MainThreadOnlyActions.Enqueue(() =>
                {
                    using (BinaryReader br2 = new BinaryReader(FileStreamOpen()))
                        ReadCardFaces(2, br2);
                });

                Memory.EnableDumpingData = true;
                ReadTIM(50, br, out _cardGameBG);
                ReadTIM(51, br, out _cardOtherBG);
                Memory.EnableDumpingData = false;
            }
        }

        private FileStream FileStreamOpen() => new FileStream(EXE_Offsets.FileName[year], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        private void ReadCardFaces(int id, BinaryReader br)
        {
            TIM2 cardtim = new TIM2(br, EXE_Offsets.TIM[year][id]);

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
        enum SymbolID
        {
            None,
            HexNumbers,
            Fire,
            Ice,
            Lightning,
            Earth,
            Poison,
            Wind,
            Water,
            Holy,
            Score,
            Buff,
            Debuff,
            Win,
            Lose,
            Draw,
            Same,
            Plus,
            Combo,
        }
        private void ReadSymbolsNumbers(int id, BinaryReader br)
        {
            const int pages = 3;
            List<TextureHandler> th = new List<TextureHandler>(pages);
            //3 pages, 256x256; inside () is palette id +1.
            //page 1 = 5 rows. first 3 rows are 16x16 grid, last 2 rows are a 24x24 grid
            //          row 1 has 11 hex numbers: 0-A (1)
            //          row 2 4 of 4 frame animations: fire(4), ice(7), lightning(10), earth(13)
            //          row 3 4 of 4 frame animations: poison(16), wind(19), water(22), holy(25)
            //          row 4 has 9 numbers: 0-9 (28)
            //          row 5 has 2 items: +1, -1 (31 or 34)
            //page 2 = 3 rows of 256x48: You Win!(2), You Lose...(5), Draw(8)
            //page 3 = 3 rows of 256x64: Same!(9), Plus!(6), Combo!(9)
            SymbolNumberEntries = new List<Entry>();
            for (int i = 0; i < 11; i++)
            {
                SymbolNumberEntries.Add(new Entry
                {
                    ID = SymbolID.HexNumbers,
                    File = 0,
                    CustomPalette = 0,
                    Location = new Vector2(i * 16, 0),
                    Size = new Vector2(16, 16),
                    NumberValue = i
                });
            }
            for (int k = 0; k < 2; k++)
                for (int j = 0; j < 4; j++)
                    for (int i = 0; i < 4; i++)
                    {
                        SymbolNumberEntries.Add(new Entry
                        {
                            ID = (SymbolID)((int)(SymbolID.Fire) + j + (k * 4)),
                            File = 0,
                            CustomPalette = checked((sbyte)(3 * (j + 1 + (k * 4)))),
                            Location = new Vector2((i + (4 * j)) * 16, 16 * (k + 1)),
                            Size = new Vector2(16, 16),
                            Frame = i,
                        });
                    }
            for (int i = 0; i < 9; i++)
            {
                SymbolNumberEntries.Add(new Entry
                {
                    ID = SymbolID.Score,
                    File = 0,
                    CustomPalette = 27,
                    Location = new Vector2(i * 24, 16 * 3),
                    Size = new Vector2(24, 24),
                    NumberValue = i
                });
            }
            for (int i = 0; i < 2; i++)
            {
                SymbolNumberEntries.Add(new Entry
                {
                    ID = (SymbolID)((int)(SymbolID.Buff) + i),
                    File = 0,
                    CustomPalette = 30,
                    Location = new Vector2(i * 24, 16 * 3 + 24),
                    Size = new Vector2(24, 24),
                    NumberValue = 1 + (-2 * i)
                });
            }
            for (int i = 0; i < 3; i++)
            {
                SymbolNumberEntries.Add(new Entry
                {
                    ID = (SymbolID)((int)(SymbolID.Win) + i),
                    File = 1,
                    CustomPalette = checked((sbyte)(1 + 3 * i)),
                    Location = new Vector2(0, 48 * i),
                    Size = new Vector2(256, 48),
                });
            }
            for (int i = 0; i < 3; i++)
            {
                SymbolNumberEntries.Add(new Entry
                {
                    ID = (SymbolID)((int)(SymbolID.Same) + i),
                    File = 2,
                    CustomPalette = checked((sbyte)(2 + 3 * i)),
                    Location = new Vector2(0, 64 * i),
                    Size = new Vector2(256, 64),
                });
            }
            TIM2 temp = new TIM2(br, EXE_Offsets.TIM[year][id]);
            temp.ForceSetClutColors(16);
            temp.ForceSetClutCount(48);
            int size = 256;
            //Rectangle pagesrc = new Rectangle(new Point(size * (page), size), new Point(size));
            //Rectangle dst;

            //using (Texture2D combined = new Texture2D(Memory.graphics.GraphicsDevice, size * 3, temp.GetHeight))
            //{
                Texture2D texture = null;
                Texture2D pagetex = null;
                sbyte CustomPalette = -1;
                sbyte File = -1;
                string filename = "";
                //string combinedfilename = $"text_combined.png";
                foreach (var e in SymbolNumberEntries)
                {
                    if (File != e.File)
                    {
                        savepagetex();
                        File = checked((sbyte)e.File);
                        pagetex = new Texture2D(Memory.graphics.GraphicsDevice, size, size);
                        filename = $"text_{File}.png";

                    }
                    if (CustomPalette != e.CustomPalette)
                    {
                        if (texture != null)
                            texture.Dispose();
                        CustomPalette = e.CustomPalette;
                        texture = temp.GetTexture((ushort)CustomPalette);
                    }
                    Color[] data = new Color[(int)(e.Width * e.Height)];
                    var src = e.GetRectangle;
                    var dst = src;
                    src.Offset(e.File * 256, 0);
                    texture.GetData(0, src, data, 0, data.Length);
                    pagetex.SetData(0, dst, data, 0, data.Length);
                    //dst = src;
                    //combined.SetData(0, dst, data, 0, data.Length);
                }
                texture.Dispose();
                savepagetex();
                void savepagetex()
                {
                    if (pagetex != null)
                    {
                        if (Memory.EnableDumpingData)
                            using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), filename), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                                pagetex.SaveAsPng(fs, pagetex.Width, pagetex.Height);

                        th.Add(TextureHandler.Create(filename, new Texture2DWrapper(pagetex)));
                    }
                }
                //if (Memory.EnableDumpingData)
                //    using (FileStream fs = new FileStream(Path.Combine(Path.GetTempPath(), combinedfilename), FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                //        combined.SaveAsPng(fs, combined.Width, combined.Height);
            //}
            //if (Memory.EnableDumpingData)
            //    Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });
            //SymbolsNumbers = new TextureHandler[temp.GetClutCount];
            //for (ushort i = 0; i < temp.GetClutCount; i++)
            //{
            //    SymbolsNumbers[i] = TextureHandler.Create(filename, temp, i);
            //    //Memory.MainThreadOnlyActions.Enqueue(SymbolsNumbers[i].Save);
            //}

            _symbolsNumbers = th.ToArray();
        }

        private void ReadTIM(int id, BinaryReader br, out TextureHandler[] tex, ushort ForceSetClutColors = 0, ushort ForceSetClutCount = 0)
        {
            TIM2 temp = new TIM2(br, EXE_Offsets.TIM[year][id]);
            if (ForceSetClutColors > 0)
                temp.ForceSetClutColors(ForceSetClutColors);
            if (ForceSetClutCount > 0)
                temp.ForceSetClutCount(ForceSetClutCount);
            string filename = $"ff8exe{id.ToString("D2")}";
            if (Memory.EnableDumpingData)
                Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });
            tex = new TextureHandler[temp.GetClutCount==0? 1: temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount || i == 0 && temp.GetClutCount == 0; i++)
            {
                tex[i] = TextureHandler.Create(filename, temp, i);
            }
        }
    }
}