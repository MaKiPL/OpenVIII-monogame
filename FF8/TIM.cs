using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FF8
{
    class TIM
    {
        private static byte[] _8BPP = { 0x10, 0x00, 0x00, 0x00, 0x09 };
        private static byte[] _4BPP = { 0x10, 0x00, 0x00, 0x00, 0x08 };
        private static byte[] _16BPP = { 0x10, 0x00, 0x00, 0x00, 0x02 };
        private static byte[] _24BPP = { 0x10, 0x00, 0x00, 0x00, 0x03 };
        private FileStream fs;
        private System.IO.BinaryReader br;
        private Texture texture;
        private Color[] colors;
        private sbyte bpp = -1;
        private bool arg0 = false;
        private string path;
        private Bitmap bmp;
        public static int _5bitColor = 255 / 31;

        public struct Texture
        {
            public ushort PaletteX;
            public ushort PaletteY;
            public ushort NumOfCluts;
            public byte[] ClutData;
            public ushort ImageOrgX;
            public ushort ImageOrgY;
            public ushort Width;
            public ushort Height;
        }

        public Texture GetParameters => texture;

        public struct Color
        {
            public byte R;
            public byte G;
            public byte B;
        }

        public TIM(string path, byte arg0 = 0, bool paramsOnly = false)
        {
            this.arg0 = arg0 != 0;
            this.path = path;
            fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            br = new BinaryReader(fs);
            texture = new Texture();
            bpp = RecognizeBPP();
            if (bpp == -1 && arg0 == 0)
            {
                Console.WriteLine("TIM: This is not TIM texture!");
                return;
            }
            if (arg0 == 0 && !paramsOnly)
            {
                ReadParameters(bpp);
                bmp = DrawTexture();
            }
            if (arg0 == 0 && paramsOnly)
            {
                ReadParameters(bpp);
            }
            br.Dispose();
            fs.Dispose();
        }

        public Bitmap GetBitmap => bmp;

        private Bitmap DrawTexture()
        {
            if (texture.ClutData != null || bpp == 16 || bpp == 24)
            {
                if (bpp == 16)
                {
                    Bitmap textureBitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format24bppRgb);
                    BitmapData bmpdata =
                        textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    IntPtr scan0Text = bmpdata.Scan0;
                    byte[] buffer = new byte[bmpdata.Stride * bmpdata.Height];
                    Marshal.Copy(scan0Text, buffer, 0, buffer.Length);

                    for (int i = 0; i < buffer.Length - 3; i += 3)
                    {
                        ushort colorbuffer = br.ReadUInt16();
                        buffer[i] = (byte)((byte)((colorbuffer >> 10) & 0x1F) * _5bitColor);
                        buffer[i + 1] = (byte)((byte)((colorbuffer >> 5) & 0x1F) * _5bitColor);
                        buffer[i + 2] = (byte)((byte)(colorbuffer & 0x1F) * _5bitColor);
                    }
                    Marshal.Copy(buffer, 0, scan0Text, buffer.Length);
                    textureBitmap.UnlockBits(bmpdata);
                    return textureBitmap;
                }
                if (bpp == 24)
                {
                    Bitmap textureBitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format24bppRgb);
                    BitmapData bmpdata =
                        textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    IntPtr scan0Text = bmpdata.Scan0;
                    byte[] buffer = new byte[bmpdata.Stride * bmpdata.Height];
                    Marshal.Copy(scan0Text, buffer, 0, buffer.Length);

                    for (int i = 0; i < buffer.Length - 1; i++)
                    {
                        buffer[i] = br.ReadByte();
                        buffer[i + 1] = br.ReadByte();
                        buffer[i + 2] = br.ReadByte();
                    }
                    Marshal.Copy(buffer, 0, scan0Text, buffer.Length);
                    textureBitmap.UnlockBits(bmpdata);
                    return textureBitmap;
                }
                if (bpp == 8)
                {
                    colors = new Color[texture.NumOfCluts * 256];
                    int col = 0;
                    for (int i = 0; i != texture.ClutData.Length; i += 2)
                    {
                        byte[] b = BitConverter.GetBytes(BitConverter.ToUInt16(texture.ClutData, i));
                        BitArray ba = new BitArray(b);
                        BitArray B = new BitArray(5);
                        BitArray R = new BitArray(5);
                        BitArray G = new BitArray(5);
                        BitArray a = new BitArray(1);
                        B[0] = ba[10]; B[1] = ba[11]; B[2] = ba[12]; B[3] = ba[13]; B[4] = ba[14]; //R
                        R[0] = ba[0]; R[1] = ba[1]; R[2] = ba[2]; R[3] = ba[3]; R[4] = ba[4]; //G
                        G[0] = ba[5]; G[1] = ba[6]; G[2] = ba[7]; G[3] = ba[8]; G[4] = ba[9]; //B
                        a[0] = ba[15]; //Alpha if 0
                        int[] b_ = new int[1]; B.CopyTo(b_, 0);
                        int[] r_ = new int[1]; R.CopyTo(r_, 0);
                        int[] g_ = new int[1]; G.CopyTo(g_, 0);
                        int[] aa = new int[1]; a.CopyTo(aa, 0);
                        double bb = Math.Round((double)((b_[0]) * (256 / 32)));
                        double rr = Math.Round((double)((r_[0]) * 256 / 32));
                        double gg = Math.Round((double)((g_[0]) * 256 / 32));
                        if (bb > 255)
                            bb--;
                        if (rr > 255)
                            rr--;
                        if (gg > 255)
                            gg--;
                        colors[col].R = (byte)bb;
                        colors[col].G = (byte)rr;
                        colors[col].B = (byte)gg;
                        col++;
                    }
                    Bitmap textureBitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format24bppRgb);
                    BitmapData bmpdata =
                        textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    IntPtr scan0Text = bmpdata.Scan0;
                    byte[] buffer = new byte[bmpdata.Stride * bmpdata.Height];
                    Marshal.Copy(scan0Text, buffer, 0, buffer.Length);

                    for (int i = 0; i < buffer.Length - 3; i += 3)
                    {
                        byte colorbuffer = br.ReadByte();
                        buffer[i] = colors[colorbuffer].R;
                        buffer[i + 1] = colors[colorbuffer].B;
                        buffer[i + 2] = colors[colorbuffer].G;
                    }
                    Marshal.Copy(buffer, 0, scan0Text, buffer.Length);
                    textureBitmap.UnlockBits(bmpdata);
                    return textureBitmap;
                }
                if (bpp == 4)
                {
                    colors = new Color[texture.NumOfCluts * 16];
                    int col = 0;
                    for (int i = 0; i != texture.ClutData.Length; i += 2)
                    {
                        byte[] b = BitConverter.GetBytes(BitConverter.ToUInt16(texture.ClutData, i));
                        BitArray ba = new BitArray(b);
                        BitArray B = new BitArray(5);
                        BitArray R = new BitArray(5);
                        BitArray G = new BitArray(5);
                        BitArray a = new BitArray(1);
                        B[0] = ba[10]; B[1] = ba[11]; B[2] = ba[12]; B[3] = ba[13]; B[4] = ba[14]; //R
                        R[0] = ba[0]; R[1] = ba[1]; R[2] = ba[2]; R[3] = ba[3]; R[4] = ba[4]; //G
                        G[0] = ba[5]; G[1] = ba[6]; G[2] = ba[7]; G[3] = ba[8]; G[4] = ba[9]; //B
                        a[0] = ba[15]; //Alpha if 0
                        int[] b_ = new int[1]; B.CopyTo(b_, 0);
                        int[] r_ = new int[1]; R.CopyTo(r_, 0);
                        int[] g_ = new int[1]; G.CopyTo(g_, 0);
                        int[] aa = new int[1]; a.CopyTo(aa, 0);
                        double bb = Math.Round((double)((b_[0]) * (256 / 32)));
                        double rr = Math.Round((double)((r_[0]) * 256 / 32));
                        double gg = Math.Round((double)((g_[0]) * 256 / 32));
                        if (bb > 255)
                            bb--;
                        if (rr > 255)
                            rr--;
                        if (gg > 255)
                            gg--;
                        colors[col].R = (byte)bb;
                        colors[col].G = (byte)rr;
                        colors[col].B = (byte)gg;
                        col++;
                    }
                    Bitmap textureBitmap = new Bitmap(texture.Width, texture.Height, PixelFormat.Format24bppRgb);
                    BitmapData bmpdata =
                        textureBitmap.LockBits(new Rectangle(0, 0, textureBitmap.Width, textureBitmap.Height),
                            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    IntPtr scan0Text = bmpdata.Scan0;
                    byte[] buffer = new byte[bmpdata.Stride * bmpdata.Height];
                    Marshal.Copy(scan0Text, buffer, 0, buffer.Length);
                    byte NEG = 0;
                    byte colorbuffer = 0;
                    for (int i = 0; i < buffer.Length - 3; i += 3)
                    {
                        if (NEG > 0)
                            colorbuffer = br.ReadByte();
                        else colorbuffer = (byte)(colorbuffer >> 4);
                        buffer[i] = colors[colorbuffer & 0xF].R;
                        buffer[i + 1] = colors[colorbuffer & 0xF].B;
                        buffer[i + 2] = colors[colorbuffer & 0xF].G;
                        NEG = (byte)~NEG;
                    }
                    Marshal.Copy(buffer, 0, scan0Text, buffer.Length);
                    textureBitmap.UnlockBits(bmpdata);
                    return textureBitmap;
                }
            }
            return null;
        }

        public Bitmap DrawRaw()
        {
            if (!this.arg0)
                throw new Exception("TIM: Tried to render RAW image with TIM loader.\nInitialize TIM without arg0");
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                if (fs.Length < 1024)
                {
                    Console.WriteLine("TIM: Image is too small to render...");
                    return new Bitmap(0, 0, PixelFormat.Format24bppRgb);
                }
                int size = 16;
                if (fs.Length >= Math.Pow(16, 2))
                    size = 16;
                if (fs.Length >= Math.Pow(32, 2))
                    size = 32;
                if (fs.Length >= Math.Pow(64, 2))
                    size = 64;
                if (fs.Length >= Math.Pow(128, 2))
                    size = 128;
                if (fs.Length >= Math.Pow(256, 2))
                    size = 256;
                if (fs.Length >= Math.Pow(512, 2))
                    size = 512;
                if (fs.Length >= Math.Pow(1024, 2))
                    size = 1024;
                if (fs.Length >= Math.Pow(2048, 2))
                    size = 2048;
                if (fs.Length >= Math.Pow(4096, 2))
                    size = 4096;
                Bitmap bmp = new Bitmap(size, size, PixelFormat.Format24bppRgb);
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, size, size), ImageLockMode.WriteOnly,
                    PixelFormat.Format24bppRgb);
                IntPtr scan0 = bmpData.Scan0;
                byte[] inBuffer = new byte[bmpData.Height * bmpData.Stride];
                Marshal.Copy(scan0, inBuffer, 0, inBuffer.Length);
                for (int i = 0; fs.Position != fs.Length - 2 && fs.Position < inBuffer.Length - 2; i++)
                    inBuffer[i] = br.ReadByte();
                Marshal.Copy(inBuffer, 0, scan0, inBuffer.Length);
                bmp.UnlockBits(bmpData);
                return bmp;

            }
        }

        private void ReadParameters(sbyte bpp)
        {
            if (bpp == 4)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                fs.Seek(2, SeekOrigin.Current);
                texture.NumOfCluts = br.ReadUInt16();
                byte[] buffer = new byte[texture.NumOfCluts * 32];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                //Console.WriteLine($"TIM: OrigX: {texture.ImageOrgX}\tOrigY:{texture.ImageOrgY}");
                texture.Width = (ushort)(br.ReadUInt16() * 4);
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp == 8)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.PaletteX = br.ReadUInt16();
                texture.PaletteY = br.ReadUInt16();
                fs.Seek(2, SeekOrigin.Current);
                texture.NumOfCluts = br.ReadUInt16();
                byte[] buffer = new byte[texture.NumOfCluts * 512];
                for (int i = 0; i != buffer.Length; i++)
                    buffer[i] = br.ReadByte();
                texture.ClutData = buffer;
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = (ushort)(br.ReadUInt16() * 2);
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp == 16)
            {
                fs.Seek(4, SeekOrigin.Current);
                texture.ImageOrgX = br.ReadUInt16();
                texture.ImageOrgY = br.ReadUInt16();
                texture.Width = br.ReadUInt16();
                texture.Height = br.ReadUInt16();
                return;
            }
            if (bpp != 24) return;
            fs.Seek(4, SeekOrigin.Current);
            texture.ImageOrgX = br.ReadUInt16();
            texture.ImageOrgY = br.ReadUInt16();
            texture.Width = (ushort)(br.ReadUInt16() / 1.5);
            texture.Height = br.ReadUInt16();
        }

        private sbyte RecognizeBPP()
        {
            byte[] buffer = br.ReadBytes(5);
            fs.Seek(3, SeekOrigin.Current);
            if (buffer.Equals(_4BPP))
                return 4;
            if (buffer[4] == 0x08)
                return 4;
            if (buffer[4] == 0x09)
                return 8;
            if (buffer[4] == 0x02)
                return 16;
            if (buffer[4] == 0x03)
                return 24;
            return -1;

        }
    }
}