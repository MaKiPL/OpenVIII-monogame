using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Classes

        private abstract class Polygon
        {
            #region Fields

            public ushort A;
            public ushort B;
            public byte bHide;
            public ushort C;
            public byte clut;
            public byte GPU;
            public byte TexturePage;
            protected byte Blue;
            protected byte Green;
            protected byte Red;
            protected byte U1;
            protected byte U2;
            protected byte U3;
            protected byte UNK;
            protected byte V1;
            protected byte V2;
            protected byte V3;

            #endregion Fields

            #region Properties

            public Color Color => new Color(Red, Green, Blue);

            public Vector2 MaxUV
            {
                get
                {
                    Vector2 vector2 = UVs[0];
                    for (int i = 1; i < UVs.Count; i++)
                        vector2 = Vector2.Max(vector2, UVs[i]);
                    return vector2;
                }
            }

            public Vector2 MinUV
            {
                get
                {
                    Vector2 vector2 = UVs[0];
                    for (int i = 1; i < UVs.Count; i++)
                        vector2 = Vector2.Min(vector2, UVs[i]);
                    return vector2;
                }
            }

            public Rectangle Rectangle
            {
                get
                {
                    Vector2 minUV = MinUV;
                    return new Rectangle(minUV.ToPoint(), (MaxUV - minUV).ToPoint());
                }
            }

            public List<Vector2> UVs { get; protected set; }

            #endregion Properties
        }

        #endregion Classes
    }
}