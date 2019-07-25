using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.Encoding.Tags;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public class Font
    {
        private Texture2D sysfnt; //21x10 characters; char is always 12x12
        private TextureHandler sysfntbig; //21x10 characters; char is always 24x24; 2 files side by side; sysfnt00 is same as sysfld00, but sysfnt00 is missing sysfnt01
        private Texture2D menuFont;

        public enum ColorID
        {
            Dark_Gray, Grey, Yellow, Red, Green, Blue, Purple, White,
            //these are darker versions that are faded to when blinking
            //Dark_GrayBlink, GreyBlink, YellowBlink, RedBlink, GreenBlink, BlueBlink, PurpleBlink, WhiteBlink
        }

        public static Dictionary<ColorID, Color> ColorID2Color = new Dictionary<ColorID, Color>
        {
            { ColorID.Dark_Gray, new Color(41,49,41,255) },
            { ColorID.Grey, new Color(148,148,164,255) },
            { ColorID.Yellow, new Color(222,222,8,255) },
            { ColorID.Red, new Color(255,24,24,255) },
            { ColorID.Green, new Color(0,255,0,255) },
            { ColorID.Blue, new Color(106,180,238,255) },
            { ColorID.Purple, new Color(255,0,255,255) },
            { ColorID.White, Color.White },
        };

        public static Dictionary<ColorID, Color> ColorID2Blink = new Dictionary<ColorID, Color>
        {
            { ColorID.Dark_Gray, new Color(12,15,12,255) },
            { ColorID.Grey, new Color(41,49,41,255) },
            { ColorID.Yellow, new Color(222,222,8,255) },
            { ColorID.Red, new Color(128,24,24,255) },
            { ColorID.Green, new Color(0,128,0,255) },
            { ColorID.Blue, new Color(53,90,128,255) },
            { ColorID.Purple, new Color(128,0,128,255) },
            { ColorID.White, new Color(148,148,164,255) }
        };

        public Font() => LoadFonts();

        public void LoadFonts()
        {
            ArchiveWorker aw = new ArchiveWorker(Memory.Archives.A_MENU);
            string sysfntTdwFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tdw"));
            string sysfntFilepath = aw.GetListOfFiles().First(x => x.ToLower().Contains("sysfnt.tex"));
            TEX tex = new TEX(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntFilepath));
            sysfnt = tex.GetTexture((int)ColorID.White);
            sysfntbig = new TextureHandler("sysfld{0:00}.tex", tex, 2, 1, (int)ColorID.White);

            TDW tim = new TDW(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntTdwFilepath), 0);
            charWidths = tim.CharWidths;
            menuFont = tim.GetTexture((ushort)ColorID.White);
        }

        private byte[] charWidths;

        public enum Type
        {
            sysFntBig,
            sysfnt,
            menuFont,
        }

        public Rectangle RenderBasicText(FF8String buffer, Vector2 pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
        {
            if (buffer == null) return new Rectangle();
            Rectangle ret = new Rectangle(pos.RoundedPoint(), new Point(0));
            Point real = pos.RoundedPoint();
            int charCountWidth = 21;
            int charSize = 12; //pixelhandler does the 2x scaling on the fly.
            Point size = (new Vector2(0, charSize) * zoom).RoundedPoint();
            int width;
            ColorID colorbak = color;
            bool blink = false;
            for (int i = 0; i < buffer.Length; i++)
            {
                byte c = buffer[i];
                if (c == (byte)FF8TextTagCode.Color)
                {
                    if (++i < buffer.Length - 1)
                    {
                        c = buffer[i];
                        blink = c >= (byte)FF8TextTagColor.Dark_GrayBlink ? true : false;
                        switch ((FF8TextTagColor)c)
                        {
                            case FF8TextTagColor.Blue:
                            case FF8TextTagColor.BlueBlink:
                                color = ColorID.Blue;
                                break;

                            case FF8TextTagColor.Green:
                            case FF8TextTagColor.GreenBlink:
                                color = ColorID.Green;
                                break;

                            case FF8TextTagColor.Grey:
                            case FF8TextTagColor.GreyBlink:
                                color = ColorID.Grey;
                                break;

                            case FF8TextTagColor.Purple:
                            case FF8TextTagColor.PurpleBlink:
                                color = ColorID.Purple;
                                break;

                            case FF8TextTagColor.Red:
                            case FF8TextTagColor.RedBlink:
                                color = ColorID.Red;
                                break;

                            case FF8TextTagColor.White:
                            case FF8TextTagColor.WhiteBlink:
                                color = colorbak;
                                // since ending color change reverts color to white. if you have a
                                // custom color set this will allow reverting to that.
                                break;

                            case FF8TextTagColor.Yellow:
                            case FF8TextTagColor.YellowBlink:
                                color = ColorID.Yellow;
                                break;

                            case FF8TextTagColor.Dark_Gray:
                            case FF8TextTagColor.Dark_GrayBlink:
                                color = ColorID.Dark_Gray;
                                break;
                        }

                        continue;
                    }
                }

                if (c == 0) continue;
                int deltaChar = (c - 32);
                if (deltaChar >= 0 && deltaChar < charWidths.Length)
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
                if (c == 0x02)// \n
                {
                    real.X = (int)pos.X;
                    real.Y += size.Y + lineSpacing;
                    continue;
                }
                Rectangle destRect = new Rectangle(real, size);
                // if you use Memory.SpriteBatchStartAlpha(SamplerState.PointClamp); you won't need
                // to trim last pixel. but it doesn't look good on low res fonts.
                if (!skipdraw)
                {
                    Rectangle sourceRect = new Rectangle((deltaChar - (verticalPosition * charCountWidth)) * charSize,
                        verticalPosition * charSize,
                        width,
                        charSize);

                    switch (whichFont)
                    {
                        case Type.menuFont:
                        case Type.sysfnt:
                            //trim pixels to remove texture filtering artifacts.
                            sourceRect.Width -= 1;
                            sourceRect.Height -= 1;

                            Memory.spriteBatch.Draw(whichFont == Type.menuFont ? menuFont : sysfnt,
                            destRect,
                            sourceRect,
                        ColorID2Color[color] * Fade);

                            if (blink)
                                Memory.spriteBatch.Draw(whichFont == Type.menuFont ? menuFont : sysfnt,
                                destRect,
                                sourceRect,
                            ColorID2Blink[color] * Fade * Menu.Blink_Amount);
                            break;

                        case Type.sysFntBig:
                            if (!sysfntbig.Modded)
                            {
                                Rectangle ShadowdestRect = new Rectangle(destRect.Location, destRect.Size);
                                ShadowdestRect.Offset(zoom);
                                sysfntbig.Draw(ShadowdestRect, sourceRect, Color.Black * Fade * .5f);
                            }
                            sysfntbig.Draw(destRect, sourceRect, ColorID2Color[color] * Fade);
                            if (blink)
                                sysfntbig.Draw(destRect, sourceRect, ColorID2Blink[color] * Fade * Menu.Blink_Amount);
                            break;
                    }
                }
                real.X += size.X;
                int curWidth = real.X - (int)pos.X;
                if (curWidth > ret.Width)
                    ret.Width = curWidth;
            }

            ret.Height = size.Y + (real.Y - (int)pos.Y);
            return ret;
        }

        public Rectangle RenderBasicText(FF8String buffer, Point pos, Vector2 zoom, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
            => RenderBasicText(buffer, pos.ToVector2(), zoom, whichFont, Fade, lineSpacing, skipdraw, color);

        public Rectangle RenderBasicText(FF8String buffer, int x, int y, float zoomWidth = 2.545455f, float zoomHeight = 3.0375f, Type whichFont = 0, float Fade = 1.0f, int lineSpacing = 0, bool skipdraw = false, ColorID color = ColorID.White)
            => RenderBasicText(buffer, new Vector2(x, y), new Vector2(zoomWidth, zoomHeight), whichFont, Fade, lineSpacing, skipdraw, color);
    }
}