using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace OpenVIII.Battle
{
    public partial class Mag
    {
        #region Classes

        public class Geometry
        {
            #region Constructors

            public Geometry()
            {
                Vertices = new List<Vector3>();
                Triangles = new List<Vector3>();
                Quads = new List<Vector4>();
            }

            #endregion Constructors

            #region Properties

            public List<Vector4> Quads { get; private set; }
            public List<Vector3> Triangles { get; private set; }
            public List<Vector3> Vertices { get; private set; }

            #endregion Properties
        }

        #endregion Classes
    }
}