using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FF8
{
    public class Font
    {
        private Texture2D sysfnt; //21x10 characters; char is always 12x12
        private TextureHandler sysfntbig; //21x10 characters; char is always 24x24; 2 files side by side; sysfnt00 is same as sysfld00, but sysfnt00 is missing sysfnt01
        private Texture2D menuFont;

        public enum ColorID
        {
            Dark_Gray, Grey, Yellow, Red, Green, Blue, Purple, White
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
            { ColorID.White, Color.White }
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

            ReadTdw(ArchiveWorker.GetBinaryFile(Memory.Archives.A_MENU, sysfntTdwFilepath));
        }

        public void ReadTdw(byte[] Tdw)
        {
            uint widthPointer = BitConverter.ToUInt32(Tdw, 0);
            uint dataPointer = BitConverter.ToUInt32(Tdw, 4);

            getWidths(Tdw, widthPointer, dataPointer - widthPointer);
            TIM2 tim = new TIM2(Tdw, dataPointer);
            menuFont = tim.GetTexture((ushort)ColorID.White);
        }

        public void getWidths(byte[] Tdw, uint offset, uint length)
        {
            using (MemoryStream os = new MemoryStream((int)length * 2))
            using (BinaryWriter bw = new BinaryWriter(os))
            using (MemoryStream ms = new MemoryStream(Tdw))
            using (BinaryReader br = new BinaryReader(ms))
            {
                //bw.Write((byte)10);//width of space
                ms.Seek(offset, SeekOrigin.Begin);
                while (ms.Position < offset + length)
                {
                    byte b = br.ReadByte();
                    byte low = (byte)(b & 0x0F);
                    byte high = (byte)(b >> 4);
                    bw.Write(low);
                    bw.Write(high);
                }
                charWidths = os.ToArray();
            }
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

            foreach (byte cs in buffer)
            {
                byte[] expanded = cs > 0xE1 && FF8String.BytetoStr.ContainsKey(cs)?
                    FF8String.BytetoStr[cs].Value : new byte[] { cs };
                foreach (byte c in expanded)
                {
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
                                break;

                            case Type.sysFntBig:
                                if (!sysfntbig.Modded)
                                {
                                    Rectangle ShadowdestRect = new Rectangle(destRect.Location, destRect.Size);
                                    ShadowdestRect.Offset(zoom);
                                    sysfntbig.Draw(ShadowdestRect, sourceRect, Color.Black * Fade * .5f);
                                }
                                sysfntbig.Draw(destRect, sourceRect, ColorID2Color[color] * Fade);
                                break;
                        }
                    }
                    real.X += size.X;
                    int curWidth = real.X - (int)pos.X;
                    if (curWidth > ret.Width)
                        ret.Width = curWidth;
                }
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