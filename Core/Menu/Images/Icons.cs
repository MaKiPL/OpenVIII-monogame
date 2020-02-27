using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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
        private new Dictionary<ID, EntryGroup> Entries = null;

        #endregion Fields

        #region Constructors

        public Icons()
        {
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

        public new uint Count => (uint)Entries.Count();

        public Rectangle DataSize { get => _dataSize; private set => _dataSize = value; }
        public new uint EntriesPerTexture => (uint)Enum.GetValues(typeof(Icons.ID)).Cast<Icons.ID>().Max();

        public new uint PaletteCount => (uint)Textures.Count();

        private new uint TextureStartOffset => 0;

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

        public Rectangle Draw(int number, NumType type, int palette, string format, Vector2 location, Vector2 scale, float fade = 1f, Font.ColorID color = Font.ColorID.White, bool blink = false, bool skipdraw = false)
        {
            if (type == NumType.sysfnt)
            {
                DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysfnt, Fade: fade, color: color, blink: blink, skipdraw: skipdraw);
                return DataSize;
            }
            else if (type == NumType.sysFntBig)
            {
                DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.sysFntBig, Fade: fade, color: color, blink: blink, skipdraw: skipdraw);
                return DataSize;
            }
            else if (type == NumType.menuFont)
            {
                DataSize = Memory.font.RenderBasicText(number.ToString(), location.ToPoint(), scale, Font.Type.menuFont, Fade: fade, color: color, blink: blink, skipdraw: skipdraw);
                return DataSize;
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
            DataSize = dst;
            if(Entries!=null)
            foreach (int i in intList)
            {
                if (!skipdraw)
                    Draw(nums[(int)type][i], palette, dst, scale, fade, blink ? Color.Lerp(Font.ColorID2Color[color], Font.ColorID2Blink[color], Menu.Blink_Amount) : Font.ColorID2Color[color]);
                float width = Entries[nums[(int)type][i]].GetRectangle.Width * scale.X;
                float height = Entries[nums[(int)type][i]].GetRectangle.Height * scale.Y;
                dst.Offset(width, 0);
                _dataSize.Width += (int)width;
                if (_dataSize.Height < (int)height)
                    _dataSize.Height = (int)height;
            }
            return DataSize;
        }

        public void Draw(Enum id, int palette, Rectangle dst, Vector2 scale, float fade = 1f, Color? color = null)
        {
            if ((ID)id != ID.None && Textures.Count>0)
                Entries?[(ID)id].Draw(Textures, palette, dst, scale, fade, color);
        }

        public override void Draw(Enum id, Rectangle dst, float fade = 1) => Draw((ID)id, 2, dst, Vector2.One, fade);

        public Entry GetEntry(Enum id, int index) => Entries?[(ID)id]?[index] ?? null;

        public override Entry GetEntry(Enum id) => Entries[(ID)id][0] ?? null;

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
                new TexProps{Filename = "icon.tex",Count = 1,Big = new List<BigTexProps>{ new BigTexProps{Filename = "iconfl{0:00}.TEX",Split = 4} } }, //0-15 palette
                new TexProps{Filename = "icon.tex",Count = 1,Colors = red,Big = new List<BigTexProps>{ new BigTexProps{Filename = "iconfl{0:00}.TEX",Split = 4,Colors = red } } },//16 palette
                new TexProps{Filename = "icon.tex",Count = 1,Colors = yellow,Big = new List<BigTexProps>{ new BigTexProps { Filename = "iconfl{0:00}.TEX", Split = 4, Colors = yellow } } }//17 palette
            };
            IndexFilename = "icon.sp1";
        }

        protected override void InitEntries(ArchiveBase aw = null)
        {
            if (Entries == null)
            {
                //read from icon.sp1
                MemoryStream ms = null;
                byte[] buffer = aw.GetBinaryFile(IndexFilename);
                if (buffer != null)
                    using (BinaryReader br = new BinaryReader(ms = new MemoryStream(buffer)))
                    {
                        Loc[] locs = new Loc[br.ReadUInt32()];
                        for (int i = 0; i < locs.Length; i++)
                        {
                            locs[i].Seek = br.ReadUInt16();
                            locs[i].Length = br.ReadUInt16();
                        }
                        Entries = new Dictionary<ID, EntryGroup>(locs.Length + 10);
                        for (int i = 0; i < locs.Length; i++)
                        {
                            ms.Seek(locs[i].Seek, SeekOrigin.Begin);
                            byte c = (byte)locs[i].Length;
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
                        ms = null;
                    }
            }
        }

        protected override void InitTextures<T>(ArchiveBase aw = null)
        {
            Textures = new List<TextureHandler>();
            for (int t = 0; t < Props.Count; t++)
            {
                byte[] buffer = aw.GetBinaryFile(Props[t].Filename);
                if (buffer != null)
                {
                    T tex = new T();
                    tex.Load(buffer);
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
        }

        protected override VertexPositionTexture_Texture2D Quad(Enum ic, byte pal, float scale = 0.25F, Box_Options options = Box_Options.Center | Box_Options.Middle, float z = 0f)
        {
            Trim(ic, pal);
            EntryGroup eg = this[(ID)ic];
            VertexPositionTexture_Texture2D r = Quad(eg[0], Textures[pal], scale, eg.Count == 1 ? options : options | Box_Options.UseOffset);

            if (eg.Count > 1)
            {
                List<VertexPositionTexture> tmp = new List<VertexPositionTexture>(r.VPT.Length * eg.Count);
                tmp.AddRange(r.VPT);
                for (int i = 1; i < eg.Count; i++)
                    tmp.AddRange(Quad(eg[0], Textures[pal], scale, options | Box_Options.UseOffset, i * 0.001f).VPT);
                return new VertexPositionTexture_Texture2D(tmp.ToArray(), r.Texture);
            }
            return r;
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

        //public VertexPositionTexture[] GenerateVPT(Vector3 v, float width, float height)
        //{
        //    Vector3[] verts = new Vector3[]
        //    {
        //        new Vector3(v.X-width/2f,v.Y+height/2f,v.Z),
        //        new Vector3(v.X+width/2f,v.Y+height/2f,v.Z),
        //        new Vector3(v.X+width/2f,v.Y-height/2f,v.Z),
        //        new Vector3(v.X-width/2f,v.Y-height/2f,v.Z),
        //    };
        //    VertexPositionTexture GetVPT(ref Debug_battleDat.Quad quad, byte i)
        //    {
        //        Vector3 GetVertex(ref Quad _quad, byte _i)
        //        {
        //            return TransformVertex(verts[_quad.GetIndex(_i)], translationPosition, rotation);
        //        }
        //        return new VertexPositionTexture(GetVertex(quad, i), quad.GetUV(i).ToVector2(preVarTex.Width, preVarTex.Height));
        //    }
        //    TempVPT[0] = TempVPT[3] = GetVPT(ref this, this[0]);
        //    TempVPT[1] = GetVPT(ref this, this[1]);
        //    TempVPT[4] = GetVPT(ref this, this[4]);
        //    TempVPT[2] = TempVPT[5] = GetVPT(ref this, this[2]);
        //}
    }
}