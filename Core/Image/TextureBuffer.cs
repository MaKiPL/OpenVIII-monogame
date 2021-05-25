using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OpenVIII
{
    public class TextureBuffer : Texture_Base, ICloneable, ICollection, IStructuralComparable, IStructuralEquatable
    {

        #region Fields

        private static bool show_message_box = true;
        #endregion Fields

        #region Constructors

        public TextureBuffer(int width, int height, bool alert = true)
        {
            Height = height;
            Width = width;
            Colors = new Color[height * width];
            Alert = alert;
        }

        #endregion Constructors

        #region Properties

        public bool Alert { get; set; }
        public Color[] Colors { get; }
        public int Count => ((ICollection)Colors).Count;
        public override byte GetBytesPerPixel => 4;
        public override int GetClutCount => 0;
        public override int GetClutSize => 0;
        public override int GetColorsCountPerPalette => 0;
        public override int GetHeight => Height;
        public override int GetOrigX => 0;
        public override int GetOrigY => 0;
        public override int GetWidth => Width;
        public int Height { get; }
        public bool IsSynchronized => Colors.IsSynchronized;
        public int Length => Colors?.Length ?? 0;
        public object SyncRoot => Colors.SyncRoot;
        public int Width { get; }

        #endregion Properties

        #region Indexers
        public Color this[int i]
        {
            get => Colors[i]; set
            {
                byte Diff(byte original, byte replace)
                {
                    if (original >= replace)
                        return (byte)(original - replace);
                    else
                        return (byte)(replace - original);
                }
                bool DiffColor(Color original, Color replace)
                {
                    const byte threshold = 10;
                    return Diff(original.R, replace.R) <= threshold
                        && Diff(original.G, replace.G) <= threshold
                        && Diff(original.B, replace.B) <= threshold
                        && Diff(original.A, replace.A) <= threshold;
                }


                if (Colors[i] != value)
                {
                    if (show_message_box && Alert && Colors[i] != Color.TransparentBlack && !DiffColor(Colors[i], value))
                    {
                        //replace exception with MessageBox.
                        var message = "There is an overlapping color!" +
                            "\nFrom:\t" + Colors[i].ToString() + 
                            "\nTo:\t" + value.ToString() +
                            "\nTo if you want to Exit press Abort and Contact OpenVIII team providing images if it's a problem!" +
                            "\nTo continue press Retry" +
                            "\nTo disable this message press Ignore";
                        var caption = "Possible Issue";
                        var buttons = MessageBoxButtons.AbortRetryIgnore;
                        
                        // Displays the MessageBox.
                        var result = MessageBox.Show(message, caption, buttons);                        
                        if(result == DialogResult.Abort)
                        {
                            Memory.QuitNextUpdate = true;
                            show_message_box = false;
                        }
                        else if(result == DialogResult.Retry)
                        {
                        }
                        else if (result == DialogResult.Ignore)
                        {
                            show_message_box = false;
                        }
                    }
                    Colors[i] = value;
                }
            }
        }

        public Color this[int x, int y]
        {
            get
            {
                var i = x + (y * Width);
                if (i < Count && i >= 0)
                    return Colors[i];
                Memory.Log.WriteLine($"{nameof(TextureBuffer)} :: this[int x, int y] => get :: {nameof(IndexOutOfRangeException)} :: {new Point(x, y)} = {i}");
                return Color.TransparentBlack; // fail silent...
            }
            set
            {
                var i = x + (y * Width);
                if (i < Count && i >= 0)
                    this[i] = value;
                else
                    Memory.Log.WriteLine($"{nameof(TextureBuffer)} :: this[int x, int y] => set :: {nameof(IndexOutOfRangeException)} :: {new Point(x, y)} = {i} :: {nameof(value)} :: {value}");
            }
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
                var pos = (x + y * Width);
                var r = new List<Color>(width * height);
                var row = 0;
                while (r.Count < width * height)
                {
                    r.AddRange(Colors.Skip(pos + row * Width).Take(width));
                    row++;
                }
                return r.ToArray();
            }
            set
            {
                for (var loopY = y; (loopY - y) < height; loopY++)
                    for (var loopX = x; (loopX - x) < width; loopX++)
                    {
                        var pos = (loopX + loopY * Width);
                        Colors[pos] = value[(loopX - x) + (loopY - y) * width];
                    }
            }
        }

        #endregion Indexers

        #region Methods

        public static explicit operator Texture2D(TextureBuffer @in)
        {
            if (Memory.Graphics?.GraphicsDevice != null && @in.Width > 0 && @in.Height > 0)
            {
                var tex = new Texture2D(Memory.Graphics.GraphicsDevice, @in.Width, @in.Height);
                @in.SetData(tex);
                return tex;
            }
            return null;
        }

        public static explicit operator Texture2DWrapper(TextureBuffer @in) => new Texture2DWrapper(@in.GetTexture());

        public static explicit operator TextureBuffer(Texture2D @in)
        {
            var texture = new TextureBuffer(@in.Width, @in.Height);

            @in.GetData(texture.Colors);
            return texture;
        }

        public static explicit operator TextureBuffer(Texture2DWrapper @in)
        {
            var texture = new TextureBuffer(@in.GetWidth, @in.GetHeight);
            var tex = @in.GetTexture();
            tex.GetData(texture.Colors);
            return texture;
        }

        public static implicit operator Color[] (TextureBuffer @in) => @in.Colors;

        public object Clone() => Colors.Clone();

        public int CompareTo(object other, IComparer comparer) => ((IStructuralComparable)Colors).CompareTo(other, comparer);

        public void CopyTo(Array array, int index) => Colors.CopyTo(array, index);

        public bool Equals(object other, IEqualityComparer comparer) => ((IStructuralEquatable)Colors).Equals(other, comparer);

        public override void ForceSetClutColors(ushort newNumOfColors)
        {
        }

        public override void ForceSetClutCount(ushort newClut)
        {
        }

        public override Color[] GetClutColors(ushort clut) => null;

        public void GetData(Texture2D tex) => tex.GetData(Colors);

        public IEnumerator GetEnumerator() => Colors.GetEnumerator();

        public int GetHashCode(IEqualityComparer comparer) => ((IStructuralEquatable)Colors).GetHashCode(comparer);

        public override Texture2D GetTexture() => (Texture2D)this;

        public override Texture2D GetTexture(Color[] colors) => (Texture2D)this;

        public override Texture2D GetTexture(ushort clut) => (Texture2D)this;

        public override void Load(byte[] buffer, uint offset = 0) => throw new NotImplementedException();

        public override void Save(string path)
        {
            using (var tex = GetTexture())
                Extended.Save_As_PNG(tex, path, tex.Width, tex.Height);
        }

        public override void SaveCLUT(string path)
        {
        }

        public void SetData(Texture2D tex) => tex.SetData(Colors);

        #endregion Methods
    }
}