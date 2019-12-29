using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII
{
    public abstract partial class SP2
    {
        #region Classes
        public class TexProps
        {
            #region Fields

            /// <summary>
            /// For big textures.
            /// </summary>
            public List<BigTexProps> Big;

            /// <summary>
            /// Override palette of texture to this and don't load other palettes. If null ignore.
            /// </summary>
            public Color[] Colors;

            /// <summary>
            /// Number of Textures
            /// </summary>
            public uint Count;

            /// Filename. To match more than one number use {0:00} or {00:00} for ones with leading
            /// zeros. </summary>
            public string Filename;

            #endregion Fields

            #region Properties

            /// <summary>
            /// Position in Filename of texture
            /// </summary>
            public uint Offset { get; set; }

            #endregion Properties

            //public TexProps(string filename, uint count, params BigTexProps[] big)
            //{
            //    Filename = filename;
            //    Count = count;
            //    if (big != null && Count != big.Length && big.Length > 0)
            //        throw new Exception($"Count of big textures should match small ones {Count} != {big.Length}");
            //    Big = big.ToList();
            //    Colors = null;
            //}

            //public TexProps(string filename, uint count, Color[] colors, params BigTexProps[] big)
            //{
            //    Filename = filename;
            //    Count = count;
            //    if (big != null && Count != big.Length && big.Length > 0)
            //        throw new Exception($"Count of big textures should match small ones {Count} != {big.Length}");
            //    Big = big.ToList();
            //    Colors = colors;
            //}

            Texture_Base.TextureType TextureType;

        }

        #endregion Classes
    }
}