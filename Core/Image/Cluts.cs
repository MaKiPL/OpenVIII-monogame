using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public class Cluts : IDictionary<byte, Color[]>
    {
        #region Fields

        private Dictionary<byte, Color[]> Clut;

        #endregion Fields

        #region Constructors

        public Cluts(Dictionary<byte, Color[]> clut, bool clone = true)
        {
            if (clone)
                Clut = clut.ToDictionary(id => id.Key, colors => (Color[])colors.Value.Clone());
            else
                Clut = clut;
        }

        public Cluts(Cluts clut) : this(clut.Clut)
        { }

        public Cluts() => Clut = new Dictionary<byte, Color[]>();

        #endregion Constructors

        #region Properties

        public IReadOnlyList<byte> ClutIDs => Clut.Keys.OrderBy(x => x).ToList();
        public int Count => ((IDictionary<byte, Color[]>)Clut).Count;
        public bool IsReadOnly => ((IDictionary<byte, Color[]>)Clut).IsReadOnly;
        public ICollection<byte> Keys => ((IDictionary<byte, Color[]>)Clut).Keys;
        public int MaxColors => Clut.Values.Max(x => x.Length);
        public ICollection<Color[]> Values => ((IDictionary<byte, Color[]>)Clut).Values;

        private byte MaxClut => Keys.Max(x => x);

        #endregion Properties

        #region Indexers

        public Color[] this[byte key] { get => ((IDictionary<byte, Color[]>)Clut)[key]; set => ((IDictionary<byte, Color[]>)Clut)[key] = value; }

        #endregion Indexers

        #region Methods

        public void Add(byte key, Color[] value) => ((IDictionary<byte, Color[]>)Clut).Add(key, value);

        public void Add(KeyValuePair<byte, Color[]> item) => ((IDictionary<byte, Color[]>)Clut).Add(item);

        public void Clear() => ((IDictionary<byte, Color[]>)Clut).Clear();

        public bool Contains(KeyValuePair<byte, Color[]> item) => ((IDictionary<byte, Color[]>)Clut).Contains(item);

        public bool ContainsKey(byte key) => ((IDictionary<byte, Color[]>)Clut).ContainsKey(key);

        public void CopyTo(KeyValuePair<byte, Color[]>[] array, int arrayIndex) => ((IDictionary<byte, Color[]>)Clut).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<byte, Color[]>> GetEnumerator() => ((IDictionary<byte, Color[]>)Clut).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<byte, Color[]>)Clut).GetEnumerator();

        public bool Remove(byte key) => ((IDictionary<byte, Color[]>)Clut).Remove(key);

        public bool Remove(KeyValuePair<byte, Color[]> item) => ((IDictionary<byte, Color[]>)Clut).Remove(item);

        public void Save(string path)
        {
            using (var clutTexture = new Texture2D(Memory.Graphics.GraphicsDevice, MaxColors, MaxClut + 1))
            {
                foreach (var _Y_Colors in Clut.OrderBy(x => x.Key))
                {
                    var colors = _Y_Colors.Value;
                    var y = _Y_Colors.Key;
                    clutTexture.SetData(0, new Rectangle(0, y, colors.Length, 1), colors, 0, colors.Length);
                }
                Extended.Save_As_PNG(clutTexture, path, clutTexture.Width, clutTexture.Height);
            }
        }

        public bool TryGetValue(byte key, out Color[] value) => ((IDictionary<byte, Color[]>)Clut).TryGetValue(key, out value);

        #endregion Methods
    }
}