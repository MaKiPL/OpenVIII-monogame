﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Card
{
    public class Game
    {
        private const int year = 2013;
        private TextureHandler[] _symbolsNumbers;
        private TextureHandler[] _cardFaces;
        private TextureHandler[] _cardGameBG;
        private TextureHandler[] _cardOtherBG;
        private List<Entry> _symbolNumberEntries;
        private List<Entry> _cardFacesEntries;

        public TextureHandler[] SymbolsNumbers { get => _symbolsNumbers; private set => _symbolsNumbers = value; }

        //private TextureHandler[] Other;
        public TextureHandler[] CardFaces { get => _cardFaces; private set => _cardFaces = value; }

        public TextureHandler[] CardGameBG { get => _cardGameBG; private set => _cardGameBG = value; }
        public TextureHandler[] CardOtherBG { get => _cardOtherBG; private set => _cardOtherBG = value; }

        public IReadOnlyList<Entry> SymbolNumberEntries => _symbolNumberEntries;
        public IReadOnlyList<Entry> CardFacesEntries => _cardFacesEntries;

        public Game()
        {
            Memory.Log.WriteLine($"{nameof(Card)} :: {nameof(Game)} :: new ");
            Memory.MainThreadOnlyActions.Enqueue(() =>
            {
                //Memory.EnableDumpingData = true;
                var input = FileStreamOpen();
                if (input != null)
                {
                    using (var br = new BinaryReader(input))
                        ReadSymbolsNumbers(0, br);
                }
                //Memory.EnableDumpingData = false;
            });

            Memory.MainThreadOnlyActions.Enqueue(() =>
            {
                var input = FileStreamOpen();
                if (input != null)
                    using (var br = new BinaryReader(input))
                        ReadCardFaces(2, br);
            });

            Memory.MainThreadOnlyActions.Enqueue(() =>
            {
                var input = FileStreamOpen();
                if (input != null)
                    using (var br = new BinaryReader(input))
                {
                    //ReadTIM(1, br,out Other);
                    ReadTIM(50, br, out _cardGameBG);
                    ReadTIM(51, br, out _cardOtherBG);
                }
            });
        }

        private FileStream FileStreamOpen()
        {
            if (File.Exists(EXE_Offsets.FileName[year]))
                return new FileStream(EXE_Offsets.FileName[year], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return null;
        }

        private void ReadCardFaces(int id, BinaryReader br)
        {
            var cardtim = new TIM2(br, EXE_Offsets.TIM[year][id]);
            using (var cardback = cardtim.GetTexture(55))
            {
                GenernateCardFaceEntries();
                cardtim.ForceSetClutColors(128);
                cardtim.ForceSetClutCount(112);
                var rows = 4;
                var size = cardback.Height / rows;
                var cols = cardback.Width / size;

                //using (Texture2D combined = new Texture2D(Memory.graphics.GraphicsDevice, cardback.Width, cardback.Height))
                //{
                var data = new Color[size * size];
                var src = new Rectangle(new Point(size * (cols - 2), size * (rows - 1)), new Point(size));
                Rectangle dst;
                //cardback.GetData(0, src, data, 0, data.Length);
                //combined.SetData(0, src, data, 0, data.Length);
                var row = 0;
                var acol = 0;
                var rc = new List<Point>(110);
                var page = 0;
                var pages = cols / 2;
                var th = new List<TextureHandler>(pages);
                ushort i = 0;
                var filename = $"cards";
                while (i < 110 && page < pages)
                {
                    var pagetex = new Texture2D(Memory.Graphics.GraphicsDevice, cardback.Height, cardback.Height);
                    for (var p = 0; p < 8 && i < 110; p++)
                    {
                        var cardface = cardtim.GetTexture(i);
                        var coloff = i % 2;
                        var even = coloff == 0;
                        if (row >= rows)
                        {
                            row = 0;
                            acol += 2;
                        }
                        var col = acol + coloff;
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
                    {
                        Extended.Save_As_PNG(pagetex, Path.Combine(Path.GetTempPath(), $"{filename}_{page}.png"), cardback.Width, cardback.Height);
                    }
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

        private void GenernateCardFaceEntries()
        {
            if (_cardFacesEntries == null || _cardFacesEntries.Count == 0)
            {
                _cardFacesEntries = new List<Entry>();
                var CardValues = (Cards.ID[])Enum.GetValues(typeof(Cards.ID));
                Array.Sort(CardValues);
                const int size = 64;
                const int numberofcardsperpage = 8;
                const int cols = 2;
                const int rows = 4;
                for (var i = 0; i < CardValues.Length && CardValues[i] <= Cards.ID.Card_Back; i++)
                {
                    new Entry
                    {
                        ID = CardValues[i],
                        File = checked((byte)(i / numberofcardsperpage)),
                        X = i % cols == 1 ? size : 0,
                        Y = ((i / cols) % rows) * size,
                        Height = size,
                        Width = size,
                        CustomPalette = checked((sbyte)i),
                    };
                }
            }
        }

        private enum SymbolID
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
            var th = new List<TextureHandler>(pages);
            //3 pages, 256x256; inside () is palette ID +1.
            //page 1 = 5 rows. first 3 rows are 16x16 grid, last 2 rows are a 24x24 grid
            //          row 1 has 11 hex numbers: 0-A (1)
            //          row 2 4 of 4 frame animations: fire(4), ice(7), lightning(10), earth(13)
            //          row 3 4 of 4 frame animations: poison(16), wind(19), water(22), holy(25)
            //          row 4 has 9 numbers: 0-9 (28)
            //          row 5 has 2 items: +1, -1 (31 or 34)
            //page 2 = 3 rows of 256x48: You Win!(2), You Lose...(5), Draw(8)
            //page 3 = 3 rows of 256x64: Same!(9), Plus!(6), Combo!(9)
            GenerateEntriesSymbolsNumbers();
            var temp = new TIM2(br, EXE_Offsets.TIM[year][id]);
            //temp.ForceSetClutColors(16);
            //temp.ForceSetClutCount(48);
            var size = 256;
            //Rectangle pagesrc = new Rectangle(new Point(size * (page), size), new Point(size));
            //Rectangle dst;

            //using (Texture2D combined = new Texture2D(Memory.graphics.GraphicsDevice, size * 3, temp.GetHeight))
            //{
            Texture2D texture = null;
            Texture2D pagetex = null;
            sbyte CustomPalette = -1;
            sbyte File = -1;
            var filename = "";
            //string combinedfilename = $"text_combined.png";
            foreach (var e in SymbolNumberEntries)
            {
                if (File != e.File)
                {
                    savepagetex();
                    File = checked((sbyte)e.File);
                    pagetex = new Texture2D(Memory.Graphics.GraphicsDevice, size, size);
                    filename = $"text_{File}.png";
                }
                if (CustomPalette != e.CustomPalette)
                {
                    if (texture != null)
                        texture.Dispose();
                    CustomPalette = e.CustomPalette;
                    texture = temp.GetTexture((ushort)CustomPalette);
                }
                var data = new Color[(int)(e.Width * e.Height)];
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
                    {
                        Extended.Save_As_PNG(pagetex, Path.Combine(Path.GetTempPath(), filename), pagetex.Width, pagetex.Height);
                    }
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

            if (Memory.EnableDumpingData)
            {
                filename = $"text_raw.png";
                for (ushort i = 0; i < temp.GetClutCount || i == 0 && temp.GetClutCount == 0; i++)
                {
                    th.Add(TextureHandler.Create(filename, temp, i));
                }
            }
            _symbolsNumbers = th.ToArray();
        }

        private void GenerateEntriesSymbolsNumbers()
        {
            if (_symbolNumberEntries == null || _symbolNumberEntries.Count == 0)
            {
                _symbolNumberEntries = new List<Entry>();
                for (var i = 0; i < 11; i++)
                {
                    _symbolNumberEntries.Add(new Entry
                    {
                        ID = SymbolID.HexNumbers,
                        File = 0,
                        CustomPalette = 0,
                        Location = new Vector2(i * 16, 0),
                        Size = new Vector2(16, 16),
                        NumberValue = i
                    });
                }
                for (var k = 0; k < 2; k++)
                    for (var j = 0; j < 4; j++)
                        for (var i = 0; i < 4; i++)
                        {
                            _symbolNumberEntries.Add(new Entry
                            {
                                ID = (SymbolID)((int)(SymbolID.Fire) + j + (k * 4)),
                                File = 0,
                                CustomPalette = checked((sbyte)(3 * (j + 1 + (k * 4)))),
                                Location = new Vector2((i + (4 * j)) * 16, 16 * (k + 1)),
                                Size = new Vector2(16, 16),
                                Frame = i,
                            });
                        }
                for (var i = 0; i < 9; i++)
                {
                    _symbolNumberEntries.Add(new Entry
                    {
                        ID = SymbolID.Score,
                        File = 0,
                        CustomPalette = 27,
                        Location = new Vector2(i * 24, 16 * 3),
                        Size = new Vector2(24, 24),
                        NumberValue = i
                    });
                }
                for (var i = 0; i < 2; i++)
                {
                    _symbolNumberEntries.Add(new Entry
                    {
                        ID = (SymbolID)((int)(SymbolID.Buff) + i),
                        File = 0,
                        CustomPalette = 30,
                        Location = new Vector2(i * 24, 16 * 3 + 24),
                        Size = new Vector2(24, 24),
                        NumberValue = 1 + (-2 * i)
                    });
                }
                for (var i = 0; i < 3; i++)
                {
                    _symbolNumberEntries.Add(new Entry
                    {
                        ID = (SymbolID)((int)(SymbolID.Win) + i),
                        File = 1,
                        CustomPalette = checked((sbyte)(1 + 3 * i)),
                        Location = new Vector2(0, 48 * i),
                        Size = new Vector2(256, 48),
                    });
                }
                for (var i = 0; i < 3; i++)
                {
                    _symbolNumberEntries.Add(new Entry
                    {
                        ID = (SymbolID)((int)(SymbolID.Same) + i),
                        File = 2,
                        CustomPalette = checked((sbyte)(2 + 3 * i)),
                        Location = new Vector2(0, 64 * i),
                        Size = new Vector2(256, 64),
                    });
                }
            }
        }

        private void ReadTIM(int id, BinaryReader br, out TextureHandler[] tex, ushort ForceSetClutColors = 0, ushort ForceSetClutCount = 0)
        {
            var temp = new TIM2(br, EXE_Offsets.TIM[year][id]);
            if (ForceSetClutColors > 0)
                temp.ForceSetClutColors(ForceSetClutColors);
            if (ForceSetClutCount > 0)
                temp.ForceSetClutCount(ForceSetClutCount);
            var filename = $"ff8exe{id.ToString("D2")}";
            if (Memory.EnableDumpingData)
                Memory.MainThreadOnlyActions.Enqueue(() => { temp.SaveCLUT(Path.Combine(Path.GetTempPath(), $"{filename}.CLUT.png")); });
            tex = new TextureHandler[temp.GetClutCount == 0 ? 1 : temp.GetClutCount];
            for (ushort i = 0; i < temp.GetClutCount || i == 0 && temp.GetClutCount == 0; i++)
            {
                tex[i] = TextureHandler.Create(filename, temp, i);
            }
        }
    }
}