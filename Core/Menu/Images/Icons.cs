using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Images of parts of most of the menus and ui.
    /// </summary>
    public sealed partial class Icons : SP2
    {
        #region Fields

        private Rectangle _dataSize;

        // ReSharper disable once InconsistentNaming
        private new Dictionary<ID, EntryGroup> Entries;
        private static readonly ID[] NumberStarts = { ID.Num_8x8_0_0, ID.Num_8x8_1_0, ID.Num_8x8_2_0, ID.Num_8x16_0_0, ID.Num_8x16_1_0, ID.Num_16x16_0_0 };
        private static ConcurrentDictionary<int, ID[]> _numbersIDs;

        #endregion Fields

        #region Enums

        public enum NumType
        {
            // ReSharper disable once UnusedMember.Global
            Num8X8,

            // ReSharper disable once UnusedMember.Global
            Num8X8A,

            Num8X8B,
            Num8X16,
            Num8X16A,

            // ReSharper disable once UnusedMember.Global
            Num16X16,

            SysFntBig,
            SysFnt,
            MenuFont
        }

        #endregion Enums

        #region Properties

        public new uint Count => (uint)Entries.Count;

        public Rectangle DataSize { get => _dataSize; private set => _dataSize = value; }

        // ReSharper disable once UnusedMember.Global
        public new uint EntriesPerTexture { get; } = (uint)Enum.GetValues(typeof(ID)).Cast<ID>().Max();

        public new uint PaletteCount => (uint)Textures.Count;

        // ReSharper disable once UnusedMember.Local
        private new uint TextureStartOffset { get; } = 0;

        #endregion Properties

        #region Indexers

        public new EntryGroup this[Enum id] => GetEntryGroup(id);

        #endregion Indexers

        #region Methods

        public static Icons Load()
        {
            Icons r = Load<Icons>();
            Memory.MainThreadOnlyActions.Enqueue(r.Trim);
            return r;
        }

        public Rectangle Draw(int number, NumType type, int palette, string format, Vector2 location, Vector2 scale, float fade = 1f, Font.ColorID color = Font.ColorID.White, bool blink = false, bool skipDraw = false)
        {
            switch (type)
            {
                case NumType.SysFnt:
                    DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysfnt, Fade: fade, color: color, blink: blink, skipdraw: skipDraw);
                    return DataSize;
                case NumType.SysFntBig:
                    DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysFntBig, Fade: fade, color: color, blink: blink, skipdraw: skipDraw);
                    return DataSize;
                case NumType.MenuFont:
                    DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.menuFont, Fade: fade, color: color, blink: blink, skipdraw: skipDraw);
                    return DataSize;
                case NumType.Num8X8:
                case NumType.Num8X8A:
                case NumType.Num8X8B:
                case NumType.Num8X16:
                case NumType.Num8X16A:
                case NumType.Num16X16:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (_numbersIDs == null)
            {
                _numbersIDs = new ConcurrentDictionary<int, ID[]>();
                int j = 0;
                foreach (ID id in NumberStarts)
                    _numbersIDs.TryAdd(j++, Enumerable.Range((int) id, 10).Select(x => checked((ID) x)).ToArray());
            }

            IEnumerable<int> intList = number.ToString(format).Select(digit => int.Parse(digit.ToString()));
            Rectangle dst = new Rectangle { Location = location.ToPoint() };
            DataSize = dst;
            if (Entries == null) return DataSize;
            foreach (int i in intList)
            {
                if (_numbersIDs == null) continue;
                if (!skipDraw)
                        Draw(_numbersIDs[(int) type][i], palette, dst, scale, fade,
                            blink
                                ? Color.Lerp(Font.ColorID2Color[color], Font.ColorID2Blink[color], Menu.Blink_Amount)
                                : Font.ColorID2Color[color]);
                float width = Entries[_numbersIDs[(int)type][i]].GetRectangle.Width * scale.X;
                float height = Entries[_numbersIDs[(int)type][i]].GetRectangle.Height * scale.Y;
                dst.Offset(width, 0);
                _dataSize.Width += (int)width;
                if (_dataSize.Height < (int)height)
                    _dataSize.Height = (int)height;
            }

            return DataSize;
        }

        public void Draw(Enum id, int palette, Rectangle dst, Vector2 scale, float fade = 1f, Color? color = null)
        {
            if ((ID)id != ID.None && Textures.Count > 0)
                Entries?[(ID)id].Draw(Textures, palette, dst, scale, fade, color);
        }

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst, Vector2.One, fade);

        public Entry GetEntry(Enum id, int index) => Entries?[(ID)id]?[index];

        public override Entry GetEntry(Enum id) => Entries[(ID)id][0];

        public EntryGroup GetEntryGroup(Enum id)
        {
            if ((ID)id != ID.None)
                return Entries?[(ID)id];
            return null;
        }

        public Color MostSaturated(Enum ic, byte pal)
        {
            if (Textures.Count > pal)
            {
                EntryGroup eg = this[(ID)ic];
                if (eg != null)
                {
                    TextureHandler tex = Textures[pal];
                    return eg.MostSaturated(tex, pal);
                }
            }
            return default;
        }

        public override void Trim(Enum ic, byte pal)
        {
            EntryGroup eg = this[(ID)ic];
            if (Textures != null && Textures.Count > pal)
                eg.Trim(Textures[pal]);
        }

        protected override void DefaultValues()
        {
            base.DefaultValues();
            Color[] red = new Color[16];
            Color[] yellow = new Color[16];
            red[15] = new Color(255, 30, 30, 255); //red
            red[14] = new Color(140, 30, 30, 255); //dark red
            red[13] = new Color(37, 37, 37, 255); //gray
            yellow[15]= new Color(222, 222, 8, 255); //yellow
            yellow[14]= new Color(131, 131, 24, 255); //dark yellow
            yellow[13]= new Color(41, 41, 41, 255); //gray
            

            //FORCE_ORIGINAL = true;
            Props = new List<TexProps>
            {
                // ReSharper disable StringLiteralTypo
                new TexProps{Filename = "icon.tex",Count = 1,Big = new List<BigTexProps>{ new BigTexProps{Filename = "iconfl{0:00}.TEX",Split = 4} } }, //0-15 palette
                new TexProps{Filename = "icon.tex",Count = 1,Colors = red,Big = new List<BigTexProps>{ new BigTexProps{Filename = "iconfl{0:00}.TEX",Split = 4,Colors = red } } },//16 palette
                new TexProps{Filename = "icon.tex",Count = 1,Colors = yellow,Big = new List<BigTexProps>{ new BigTexProps { Filename = "iconfl{0:00}.TEX", Split = 4, Colors = yellow } } }//17 palette
                // ReSharper restore StringLiteralTypo
            };
            IndexFilename = "icon.sp1";
        }

        protected override void InitEntries(ArchiveBase aw = null)
        {
            if (Entries != null) return;
            //read from icon.sp1
            MemoryStream ms;
            byte[] buffer = aw.GetBinaryFile(IndexFilename);
            if (buffer == null) return;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
            {
                Loc[] locations = new Loc[br.ReadUInt32()];
                for (int i = 0; i < locations.Length; i++)
                {
                    locations[i].Seek = br.ReadUInt16();
                    locations[i].Length = br.ReadUInt16();
                }
                Entries = new Dictionary<ID, EntryGroup>(locations.Length + 10);
                for (int i = 0; i < locations.Length; i++)
                {
                    ms.Seek(locations[i].Seek, SeekOrigin.Begin);
                    byte c = (byte)locations[i].Length;
                    Entries[(ID)i] = new EntryGroup(c);
                    for (int e = 0; e < c; e++)
                    {
                        Entry tmp = new Entry();
                        tmp.LoadfromStreamSP1(br);
                        tmp.Part = (byte)e;
                        tmp.SetLoc(locations[i]);
                        Entries[(ID)i].Add(tmp);
                    }
                }
            }
        }

        protected override void InitTextures<T>(ArchiveBase aw = null)
        {
            Textures = new List<TextureHandler>();
            foreach (TexProps t1 in Props)
            {
                byte[] buffer = aw.GetBinaryFile(t1.Filename);
                if (buffer == null) continue;
                T tex = new T();
                tex.Load(buffer);
                if (t1.Colors == null || t1.Colors.Length == 0)
                {
                    for (ushort i = 0; i < tex.GetClutCount; i++)
                    {
                        if (ForceOriginal == false && t1.Big != null && t1.Big.Count > 0)
                            Textures.Add(TextureHandler.Create(t1.Big[0].Filename, tex, 2, t1.Big[0].Split / 2, i));
                        else
                            Textures.Add(TextureHandler.Create(t1.Filename, tex, 1, 1, i));
                    }
                }
                else
                {
                    if (ForceOriginal == false && t1.Big != null && t1.Big.Count > 0)
                        Textures.Add(TextureHandler.Create(t1.Big[0].Filename, tex, 2, t1.Big[0].Split / 2, (ushort)Textures.Count, colors: t1.Big[0].Colors ?? t1.Colors));
                    else
                        Textures.Add(TextureHandler.Create(t1.Filename, tex, 1, 1, (ushort)Textures.Count, colors: t1.Colors));
                }
            }
        }

        protected override VertexPositionTexture_Texture2D Quad(Enum ic, byte pal, float scale = 0.25F, Box_Options options = Box_Options.Center | Box_Options.Middle, float z = 0f)
        {
            Trim(ic, pal);
            EntryGroup eg = this[(ID)ic];
            VertexPositionTexture_Texture2D r = Quad(eg[0], Textures[pal], scale, eg.Count == 1 ? options : options | Box_Options.UseOffset);

            if (eg.Count <= 1) return r;
            List<VertexPositionTexture> tmp = new List<VertexPositionTexture>(r.VPT.Length * eg.Count);
            tmp.AddRange(r.VPT);
            for (int i = 1; i < eg.Count; i++)
                tmp.AddRange(Quad(eg[0], Textures[pal], scale, options | Box_Options.UseOffset, i * 0.001f).VPT);
            return new VertexPositionTexture_Texture2D(tmp.ToArray(), r.Texture);
        }

        private void Trim()
        {
            Trim(ID.Bar_Fill, 5);

            //trim checks to see if it's ran once before.
            //so no need to check if it's already ran.
            //will throw exception if not in main thread.
            for (byte i = 0; i <= 7; i++)
                Trim(ID._0_Hit_ + i, 2);
            Trim(ID.Trigger_, 2);
            Trim(ID.Perfect__, 2);
            Trim(ID.Renzokeken_Seperator, 6);
        }

        #endregion Methods
    }
}