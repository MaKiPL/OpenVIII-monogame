using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Font Module
    /// </summary>
    /// <remarks>
    /// 21x10 characters; char is always 24x24; 2 files side by side; sysfnt00 is same as sysfld00,
    /// but sysfnt00 is missing sysfnt01
    /// </remarks>
    public class Font
    {
        #region Fields

        public static Dictionary<ColorID, Color> ColorID2Blink = new Dictionary<ColorID, Color>
        {
            { ColorID.Dark_Grey, new Color(12,15,12,255) },
            { ColorID.Grey, new Color(41,49,41,255) },
            { ColorID.Yellow, new Color(222,222,8,255) },
            { ColorID.Red, new Color(128,24,24,255) },
            { ColorID.Green, new Color(0,128,0,255) },
            { ColorID.Blue, new Color(53,90,128,255) },
            { ColorID.Purple, new Color(128,0,128,255) },
            { ColorID.White, new Color(148,148,164,255) }
        };

        public static Dictionary<ColorID, Color> ColorID2Color = new Dictionary<ColorID, Color>
        {
            { ColorID.Dark_Grey, new Color(41,49,41,255) },
            { ColorID.Grey, new Color(148,148,164,255) },
            { ColorID.Yellow, new Color(222,222,8,255) },
            { ColorID.Red, new Color(255,24,24,255) },
            { ColorID.Green, new Color(0,255,0,255) },
            { ColorID.Blue, new Color(106,180,238,255) },
            { ColorID.Purple, new Color(255,0,255,255) },
            { ColorID.White, Color.White },
        };

        private byte[] charWidths;
        private Texture2D menuFont;
        private Texture2D sysfnt; //21x10 characters; char is always 12x12
        private TextureHandler sysfntbig;

        #endregion Fields

        #region Constructors

        public Font() => LoadFonts();

        #endregion Constructors

        #region Enums

        public enum ColorID
        {
            Dark_Grey, Grey, Yellow, Red, Green, Blue, Purple, White,
            //these are darker versions that are faded to when blinking
        }

        public enum Type
        {
            sysFntBig,
            sysfnt,
            menuFont,
        }

        #endregion Enums

        #region Methods

        public void LoadFonts()
        {
            Memory.Log.WriteLine($"{nameof(Font)} :: {nameof(LoadFonts)} ");
            ArchiveBase aw = ArchiveWorker.Load(Memory.Archives.A_MENU);
            byte[] bufferTex = aw.GetBinaryFile("sysfnt.tex");
            TEX tex = new TEX(bufferTex);
            sysfnt = tex.GetTexture((int)ColorID.White);
            sysfntbig = TextureHandler.Create("sysfld{0:00}.tex", tex, 2, 1, (int)ColorID.White);

            byte[] bufferTDW = aw.GetBinaryFile("sysfnt.tdw");
            TDW tim = new TDW(bufferTDW);
            charWidths = tim.CharWidths;
            menuFont = tim.GetTexture((ushort)ColorID.White);
            
        }

        public Rectangle RenderBasicText(FF8String buffer, Vector2 pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color_ = ColorID.White, bool blink = false)
        {
            ColorID colorbak = color_;
            Color color = ColorID2Color[color_];
            Color faded_color = ColorID2Blink[color_];
            if (buffer == null) return new Rectangle();
            Rectangle ret = new Rectangle(pos.RoundedPoint(), new Point(0));
            Rectangle destRect = Rectangle.Empty;
            Point real = pos.RoundedPoint();
            int charCountWidth = 21;
            int charSize = 12; //pixelhandler does the 2x scaling on the fly.
            Point size = (new Vector2(0, charSize) * zoom).RoundedPoint();
            Point baksize = size;
            int width = 0;
            bool skipletter = false;
            for (int i = 0; i < buffer.Length; i++)
            {
                size = baksize;
                byte c = buffer[i];
                if (c == 0) continue;
                else if (c == (byte)FF8TextTagCode.Dialog)
                {
                    if (++i < buffer.Length - 1)
                    {
                        c = buffer[i];
                        switch ((FF8TextTagDialog)c)
                        {
                            // Most of these should be replaced before it gets here becuase they have
                            // values set by other objects.
                            case FF8TextTagDialog.CustomICON:
                                DrawIcon(buffer, zoom, Fade, skipdraw, real, ref size, ref skipletter, ref i, ref c);
                                break;
                        }
                        //if (!skipletter)
                        SetRetRec(pos, ref ret, ref real, size);
                        continue;
                    }
                }
                else if (c == (byte)FF8TextTagCode.Key)
                {
                    if (++i < buffer.Length - 1)
                    {
                        FF8TextTagKey k = (FF8TextTagKey)buffer[i];
                        FF8String str = Input2.ButtonString(k);
                        Rectangle retpos = RenderBasicText(str, real, zoom, whichFont, Fade, lineSpacing, skipdraw, ColorID.Green);
                        size.X = retpos.Width;
                        //size.Y = retpos.Height;
                        //real.X += retpos.Width;
                        //TODO add key/controller input icons/text here.
                        //if (!skipletter)
                        SetRetRec(pos, ref ret, ref real, size);
                        continue;
                    }
                }
                else if (c == (byte)FF8TextTagCode.Color)
                {
                    if (++i < buffer.Length - 1)
                    {
                        c = buffer[i];
                        blink = c >= (byte)FF8TextTagColor.Dark_GrayBlink ? true : false;
                        GetColorFromTag(c, out Color? nc, out Color? fc);
                        color = nc ?? ColorID2Color[colorbak];
                        faded_color = fc ?? ColorID2Blink[colorbak];
                        SetRetRec(pos, ref ret, ref real, size);
                        continue;
                    }
                }
                else if (c == (byte)FF8TextTagCode.Line && NewLine(pos, lineSpacing, ref real, size))
                {
                    SetRetRec(pos, ref ret, ref real, size);
                    continue;
                }
                if (!skipletter)
                {
                    int deltaChar = GetDeltaChar(c);
                    if (deltaChar >= 0 && charWidths != null && deltaChar < charWidths.Length)
                    {
                        width = charWidths[deltaChar];
                        size.X = (int)(charWidths[deltaChar] * zoom.X);
                    }
                    else
                    {
                        width = charSize;
                        size.X = (int)(charSize * zoom.X);
                    }
                    Point curSize = size;
                    int verticalPosition = deltaChar / charCountWidth;
                    //i.e. 1280 is 100%, 640 is 50% and therefore 2560 is 200% which means multiply by 0.5f or 2.0f

                    destRect = new Rectangle(real, size);
                    if (!skipdraw)
                    {
                        Rectangle sourceRect = new Rectangle((deltaChar - (verticalPosition * charCountWidth)) * charSize,
                            verticalPosition * charSize,
                            width,
                            charSize);
                        DrawLetter(whichFont, Fade, blink ? Color.Lerp(color, faded_color, Menu.Blink_Amount) : color, destRect, sourceRect);
                    }
                }
                skipletter = false;
                SetRetRec(pos, ref ret, ref real, size);
            }

            ret.Height = size.Y + (real.Y - (int)pos.Y);
            return ret;
        }

        private static void SetRetRec(Vector2 pos, ref Rectangle ret, ref Point real, Point size)
        {
            real.X += size.X;
            int curWidth = real.X - (int)pos.X;
            if (curWidth > ret.Width)
                ret.Width = curWidth;
        }

        private static void DrawIcon(FF8String buffer, Vector2 zoom, float Fade, bool skipdraw, Point real, ref Point size, ref bool skipletter, ref int i, ref byte c)
        {
            if (i + 3 < buffer.Length)
            {
                c = buffer[++i];
                short ic = c;
                c = buffer[++i];
                ic |= (short)(c << 8);
                byte pal = buffer[++i];
                Memory.Icons.Trim((Icons.ID)ic, pal);
                EntryGroup icon = Memory.Icons[(Icons.ID)ic];
                //Vector2 scale = Memory.Icons.GetTexture((Icons.ID)ic).ScaleFactor;
                if (icon != null)
                {
                    float adj = (12 / (float)(icon.Height));
                    Vector2 scale = new Vector2(adj * zoom.X);
                    size.X = (int)(icon.Width * scale.X);
                    size.Y = (int)(icon.Height * scale.X);
                    Rectangle destRect = new Rectangle(real, size);
                    //real.X += size.X;
                    //skipletter = true;
                    if (!skipdraw)
                        Memory.Icons.Draw((Icons.ID)ic, pal, destRect, Vector2.Zero, Fade);
                }
            }
        }

        public Rectangle RenderBasicText(FF8String buffer, Point pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White, bool blink = false)
            => RenderBasicText(buffer, pos.ToVector2(), zoom, whichFont, Fade, lineSpacing, skipdraw, color, blink);

        public Rectangle RenderBasicText(FF8String buffer, int x, int y, float zoomWidth = 2.545455f, float zoomHeight = 3.0375f, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White, bool blink = false)
            => RenderBasicText(buffer, new Vector2(x, y), new Vector2(zoomWidth, zoomHeight), whichFont, Fade, lineSpacing, skipdraw, color,blink);

        private static void GetColorFromTag(byte c, out Color? color, out Color? faded_color)
        {
            color = null;
            faded_color = null;
            ColorID? cid = null;
            switch ((FF8TextTagColor)c)
            {
                case FF8TextTagColor.Blue:
                case FF8TextTagColor.BlueBlink:
                    cid = ColorID.Blue;
                    break;

                case FF8TextTagColor.Green:
                case FF8TextTagColor.GreenBlink:
                    cid = ColorID.Green;
                    break;

                case FF8TextTagColor.Grey:
                case FF8TextTagColor.GreyBlink:
                    cid = ColorID.Grey;
                    break;

                case FF8TextTagColor.Purple:
                case FF8TextTagColor.PurpleBlink:
                    cid = ColorID.Purple;
                    break;

                case FF8TextTagColor.Red:
                case FF8TextTagColor.RedBlink:
                    cid = ColorID.Red;
                    break;

                case FF8TextTagColor.White:
                case FF8TextTagColor.WhiteBlink:
                    // since ending cid change reverts cid to white. if you have a custom cid set
                    // this will allow reverting to that.
                    break;

                case FF8TextTagColor.Yellow:
                case FF8TextTagColor.YellowBlink:
                    cid = ColorID.Yellow;
                    break;

                case FF8TextTagColor.Dark_Gray:
                case FF8TextTagColor.Dark_GrayBlink:
                    cid = ColorID.Dark_Grey;
                    break;
            }
            if (cid.HasValue)
            {
                color = ColorID2Color[cid.Value];
                faded_color = ColorID2Blink[cid.Value];
            }
        }

        private static int GetDeltaChar(byte c) => (c - 32);

        private static bool NewLine(Vector2 pos, int lineSpacing, ref Point real, Point size)
        {
            bool gonext;
            real.X = (int)pos.X;
            real.Y += size.Y + lineSpacing;
            gonext = true;
            return gonext;
        }

        private void DrawLetter(Type whichFont, float Fade, Color color, Rectangle destRect, Rectangle sourceRect)
        {
            switch (whichFont)
            {
                case Type.menuFont:
                case Type.sysfnt:
                    // if you use Memory.SpriteBatchStartAlpha(SamplerState.PointClamp); you won't need
                    // to trim last pixel. but it doesn't look good on low res fonts.
                    //trim pixels to remove texture filtering artifacts.
                    //sourceRect.Width -= 1;
                    //sourceRect.Height -= 1;

                    Memory.spriteBatch.Draw(whichFont == Type.menuFont ? menuFont : sysfnt,
                    destRect,
                    sourceRect,
                color * Fade);

                    break;

                case Type.sysFntBig:
                    if (sysfntbig != null)
                    {
                        if (!sysfntbig.Modded)
                        {
                            Rectangle ShadowdestRect = new Rectangle(destRect.Location, destRect.Size);
                            ShadowdestRect.Offset(2, 2);
                            sysfntbig.Draw(ShadowdestRect, sourceRect, Color.Black * Fade * .5f);
                        }
                        sysfntbig.Draw(destRect, sourceRect, color * Fade);
                    }
                    break;
            }
        }

        #endregion Methods
    }
}