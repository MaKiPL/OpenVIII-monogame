using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Interfaces

        public interface IPolygon
        {
            #region Properties

            ushort A { get; }
            ushort B { get; }
            ushort C { get; }
            byte Clut { get; }
            GPU GPU { get; }
            byte TexturePage { get; }


            byte Hide { get; }
            Color Color { get; }

            Vector2[] UVs { get; }
            #endregion Properties
        }

        public interface IQuad
        {
            #region Properties

            ushort D { get; }

            #endregion Properties
        }

        #endregion Interfaces

       


    }
[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public static class PolygonExt
{
    #region Fields


    #endregion Fields


    public static Vector2 MaxUV(this Stage.IPolygon v)
    {
        if (v.UVs == null || v.UVs.Length <= 2) return Vector2.Zero;
        var vector2 = v.UVs[0];
        for (var i = 1; i < v.UVs.Length; i++)
            vector2 = Vector2.Max(vector2, v.UVs[i]);
        return vector2;

    }

    public static Vector2 MinUV(this Stage.IPolygon v)
    {
        if (v.UVs == null || v.UVs.Length <= 2) return Vector2.Zero;
        var vector2 = v.UVs[0];
        for (var i = 1; i < v.UVs.Length; i++)
            vector2 = Vector2.Min(vector2, v.UVs[i]);
        return vector2;

    }

    public static Rectangle Rectangle(this Stage.IPolygon v)
    {
        Vector2 minUV = v.MinUV();
        var maxUV = v.MaxUV();
        return new Rectangle(minUV.ToPoint(), (maxUV - minUV).ToPoint());

    }
}
}