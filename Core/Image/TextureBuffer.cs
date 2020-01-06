using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class TextureBuffer : Texture_Base, ICloneable, ICollection, IEnumerable, IStructuralComparable, IStructuralEquatable
    {
        #region Fields

        private Color[] colors;
        
        #endregion Fields

        #region Constructors

        public TextureBuffer(int width, int height, bool alert = true)
        {
            Height = height;
            Width = width;
            colors = new Color[height * width];
            Alert = alert;
        }

        #endregion Constructors

        #region Properties

        public bool Alert { get; set; }
        public Color[] Colors { get => colors; private set => colors = value; }
        public int Height { get; private set; }
        public int Length => colors?.Length ?? 0;
        public int Width { get; private set; }

        public override int GetClutCount => 0;

        public override byte GetBytesPerPixel => 4;

        public override int GetClutSize => 0;

        public override int GetColorsCountPerPalette => 0;

        public override int GetHeight => Height;

        public override int GetOrigX => 0;

        public override int GetOrigY => 0;

        public override int GetWidth => Width;

        public int Count => ((ICollection)colors).Count;

        public object SyncRoot => colors.SyncRoot;

        public bool IsSynchronized => colors.IsSynchronized;

        #endregion Properties

        #region Indexers

        public Color this[int i]
        {
            get => colors[i]; set
            {
                if (Alert && colors[i] != Color.TransparentBlack)
                    throw new Exception("Color is set!");
                colors[i] = value;
            }
        }

        public Color this[int x, int y]
        {
            get => colors[x + (y * Width)]; set => this[x + (y * Width)] = value;
        }

        public Color[] this[Rectangle rectangle]
        {
            get => this[rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height];
            set => this[rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height] = value;
        }

        public Color[] this[int x, int y, int width, int height]
        {
            get
            {
                int pos = (x + y * Width);
                List<Color> r = new List<Color>(width * height);
                int row = 0;
                while (r.Count < width * height)
                {
                    r.AddRange(colors.Skip(pos + row * Width).Take(width));
                    row++;
                }
                return r.ToArray();
            }
            set
            {
                for (int _y = y; (_y - y) < height; _y++)
                    for (int _x = x; (_x - x) < width; _x++)
                    {
                        int pos = (_x + _y * Width);
                        colors[pos] = value[(_x - x) + (_y - y) * width];
                    }
            }
        }

        #endregion Indexers

        #region Methods

        public static explicit operator Texture2D(TextureBuffer @in)
        {
            Texture2D tex = new Texture2D(Memory.graphics.GraphicsDevice, @in.Width, @in.Height);
            @in.SetData(tex);
            return tex;
        }
        public static explicit operator Texture2DWrapper(TextureBuffer @in)
        {
            return new Texture2DWrapper(@in.GetTexture());
        }

        public static explicit operator TextureBuffer(Texture2D @in)
        {
            TextureBuffer texture = new TextureBuffer(@in.Width, @in.Height);

            @in.GetData(texture.colors);
            return texture;
        }
        public static explicit operator TextureBuffer(Texture2DWrapper @in)
        {
            TextureBuffer texture = new TextureBuffer(@in.GetWidth, @in.GetHeight);
            var tex = @in.GetTexture();
            tex.GetData(texture.colors);
            return texture;
        }

        public static implicit operator Color[] (TextureBuffer @in) => @in.colors;

        public void SetData(Texture2D tex) => tex.SetData(colors);

        public override void ForceSetClutColors(ushort newNumOfColours)
        {
        }

        public override void ForceSetClutCount(ushort newClut)
        {
        }

        public override Color[] GetClutColors(ushort clut) => null;

        public override Texture2D GetTexture() => (Texture2D)this;

        public override Texture2D GetTexture(Color[] colors = null) => (Texture2D)this;

        public override Texture2D GetTexture(ushort? clut = null) => (Texture2D)this;

        public override void Save(string path)
        {
            using (Texture2D tex = GetTexture())
            using (FileStream fs = File.Create(path))
                tex.SaveAsPng(fs, tex.Width, tex.Height);
        }

        public override void SaveCLUT(string path)
        {
        }

        public override void Load(byte[] buffer, uint offset = 0) => throw new NotImplementedException();
        public bool Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)colors).Equals(other, comparer);
        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)colors).GetHashCode(comparer);
        public int CompareTo(object other, IComparer comparer) => ((IStructuralComparable)colors).CompareTo(other, comparer);
        public IEnumerator GetEnumerator() => colors.GetEnumerator();
        public void CopyTo(Array array, int index) => colors.CopyTo(array, index);
        public object Clone() => colors.Clone();

        #endregion Methods
    }
}