using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Images of parts of most of the menus and ui.
    /// </summary>
    public partial class Icons : SP2
    {
        #region Fields

        private new Dictionary<ID, EntryGroup> Entries = null;

        #endregion Fields

        #region Constructors

        public Icons()
        {
            Color[] red = new Color[256];
            red[15] = new Color(255, 30, 30, 255); //red
            red[14] = new Color(140, 30, 30, 255); //dark red
            red[13] = new Color(37, 37, 37, 255); //gray
            Color[] yellow = new Color[256];
            yellow[15] = new Color(222, 222, 8, 255); //yellow
            yellow[14] = new Color(131, 131, 24, 255); //dark yellow
            yellow[13] = new Color(41, 41, 41, 255); //gray

            //FORCE_ORIGINAL = true;
            Props = new List<TexProps>()
            {
                new TexProps("icon.tex",1,new BigTexProps("iconfl{0:00}.TEX",4)), //0-15 palette
                new TexProps("icon.tex",1,red, new BigTexProps("iconfl{0:00}.TEX",4,red)),//16 palette
                new TexProps("icon.tex",1,yellow, new BigTexProps("iconfl{0:00}.TEX",4,yellow))//17 palette
            };
            IndexFilename = "icon.sp1";
            Init();
        }

        #endregion Constructors

        #region Enums

        public enum NumType
        {
            Num_8x8_0,
            Num_8x8_1,
            Num_8x8_2,
            Num_8x16_0,
            Num_8x16_1,
            Num_16x16_0,
            sysFntBig,
            sysfnt,
            menuFont,
        }

        #endregion Enums

        #region Properties

        public new uint EntriesPerTexture => (uint)Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max();
        public new uint PaletteCount => (uint)Textures.Count();
        public new uint Count => (uint)Entries.Count();
        private new uint TextureStartOffset => 0;

        #endregion Properties

        #region Indexers

        public new EntryGroup this[Enum id] => GetEntryGroup(id);

        #endregion Indexers

        #region Methods

        public void Draw(int number, NumType type, int palette, string format, Vector2 location, Vector2 scale, float fade = 1f, Font.ColorID color = Font.ColorID.White, bool blink = false)
        {
            if (type == NumType.sysfnt)
            {
                Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysfnt, Fade: fade, color: color, blink: blink);
                return;
            }
            else if (type == NumType.sysFntBig)
            {
                Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysFntBig, Fade: fade, color: color, blink: blink);
                return;
            }
            else if (type == NumType.menuFont)
            {
                Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.menuFont, Fade: fade, color: color, blink: blink);
                return;
            }
            ID[] numberstarts = { ID.Num_8x8_0_0, ID.Num_8x8_1_0, ID.Num_8x8_2_0, ID.Num_8x16_0_0, ID.Num_8x16_1_0, ID.Num_16x16_0_0 };
            List<ID>[] nums = new List<ID>[numberstarts.Length];
            int j = 0;
            foreach (ID id in numberstarts)
            {
                nums[j] = new List<ID>(10);
                for (byte i = 0; i < 10; i++)
                {
                    nums[j].Add(id + i);
                }
                j++;
            }
            IEnumerable<int> intList = number.ToString(format).Select(digit => int.Parse(digit.ToString()));
            Rectangle dst = new Rectangle { Location = location.ToPoint() };
            foreach (int i in intList)
            {
                Draw(nums[(int)type][i], palette, dst, scale, fade, blink ? Color.Lerp(Font.ColorID2Color[color], Font.ColorID2Blink[color], Menu.Blink_Amount) : Font.ColorID2Color[color]);
                dst.Offset(Entries[nums[(int)type][i]].GetRectangle.Width * scale.X, 0);
            }
        }

        public void Draw(Enum id, int palette, Rectangle dst, Vector2 scale, float fade = 1f, Color? color = null)
        {
            if ((ID)id != ID.None)
                Entries[(ID)id].Draw(Textures, palette, dst, scale, fade,color);
        }

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst, Vector2.One, fade);

        public Entry GetEntry(Enum id, int index) => Entries[(ID)id][index] ?? null;

        public override Entry GetEntry(Enum id) => Entries[(ID)id][0] ?? null;

        public EntryGroup GetEntryGroup(Enum id)
        {
            if ((ID)id != ID.None)
                return Entries[(ID)id] ?? null;
            return null;
        }

        public new void Trim(Enum ic, byte pal)
        {
            EntryGroup eg = this[(ID)ic];
            eg.Trim(Textures[pal]);
        }
        public Color AverageColor(Enum ic, byte pal)
        {
            EntryGroup eg = this[(ID)ic];
            return eg.AverageColor(Textures[pal]);
        }

        protected override void InitEntries(ArchiveWorker aw = null)
        {
            if (Entries == null)
            {
                //read from icon.sp1
                using (MemoryStream ms = new MemoryStream(ArchiveWorker.GetBinaryFile(ArchiveString,
                    aw.GetListOfFiles().First(x => x.IndexOf(IndexFilename, StringComparison.OrdinalIgnoreCase) >= 0))))
                {
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        Loc[] locs = new Loc[br.ReadUInt32()];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].seek = br.ReadUInt16();
                            locs[i].length = br.ReadUInt16();
                        }
                        Entries = new Dictionary<ID, EntryGroup>(locs.Length + 10);
                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].seek, SeekOrigin.Begin);
                            byte c = (byte)locs[i].length;
                            Entries[(ID)i] = new EntryGroup(c);
                            for (int e = 0; e < c; e++)
                            {
                                Entry tmp = new Entry();
                                tmp.LoadfromStreamSP1(br);
                                tmp.Part = (byte)e;
                                tmp.SetLoc(locs[i]);
                                Entries[(ID)i].Add(tmp);
                            }
                        }
                    }
                }
            }
        }

        protected override void InitTextures(ArchiveWorker aw = null)
        {
            Textures = new List<TextureHandler>();
            for (int t = 0; t < Props.Count; t++)
            {
                TEX tex;
                tex = new TEX(ArchiveWorker.GetBinaryFile(ArchiveString,
                    aw.GetListOfFiles().First(x => x.IndexOf(Props[t].Filename, StringComparison.OrdinalIgnoreCase) >= 0)));
                if (Props[t].Colors == null || Props[t].Colors.Length == 0)
                {
                    for (ushort i = 0; i < tex.GetClutCount; i++)
                    {
                        if (FORCE_ORIGINAL == false && Props[t].Big != null && Props[t].Big.Count > 0)
                            Textures.Add(TextureHandler.Create(Props[t].Big[0].Filename, tex, 2, Props[t].Big[0].Split / 2, i));
                        else
                            Textures.Add(TextureHandler.Create(Props[t].Filename, tex, 1, 1, i));
                    }
                }
                else
                {
                    if (FORCE_ORIGINAL == false && Props[t].Big != null && Props[t].Big.Count > 0)
                        Textures.Add(TextureHandler.Create(Props[t].Big[0].Filename, tex, 2, Props[t].Big[0].Split / 2, (ushort)Textures.Count, colors: Props[t].Big[0].Colors ?? Props[t].Colors));
                    else
                        Textures.Add(TextureHandler.Create(Props[t].Filename, tex, 1, 1, (ushort)Textures.Count, colors: Props[t].Colors));
                }
            }
        }

        #endregion Methods
    }
}