using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Two Verts if you cross them it
    /// </summary>
    public class Gateway
    {
        #region Fields

        private WalkMesh.Vert Destination;
        private ushort FieldID;
        private uint[] unknown;
        private WalkMesh.Vert[] Verts;

        #endregion Fields

        #region Constructors

        public Gateway()
        {
            Verts = new WalkMesh.Vert[2];
            unknown = new uint[3];
        }

        #endregion Constructors

        #region Methods

        public static Gateway Read(BinaryReader br, int type)
        {
            Gateway g = new Gateway();
            g.Verts[0] = new WalkMesh.Vert
            {
                x = br.ReadInt16(),
                z = br.ReadInt16(),
                y = br.ReadInt16(),
            };
            g.Verts[1] = new WalkMesh.Vert
            {
                x = br.ReadInt16(),
                z = br.ReadInt16(),
                y = br.ReadInt16(),
            };
            g.Destination = new WalkMesh.Vert
            {
                x = br.ReadInt16(),
                z = br.ReadInt16(),
                y = br.ReadInt16(),
            };
            g.FieldID = br.ReadUInt16();
            foreach (int i in Enumerable.Range(0, type == 0 ? 3 : 1))
                g.unknown[i] = br.ReadUInt32();
            return g;
        }

        #endregion Methods
    }
}