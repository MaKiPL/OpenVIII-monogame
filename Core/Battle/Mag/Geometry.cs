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

            public List<Vector4> Quads { get;  }
            public List<Vector3> Triangles { get;  }
            public List<Vector3> Vertices { get;  }

            #endregion Properties
        }

        #endregion Classes
    }
}