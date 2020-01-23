using System.IO;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Where door will open or event might fire if you crosses line.
    /// </summary>
    public class Trigger
    {
        #region Fields

        public byte DoorID = 0xFF;
        public WalkMesh.Vert[] Verts;
        private byte[] unknown;

        #endregion Fields

        #region Constructors

        public Trigger() => Verts = new WalkMesh.Vert[2];

        #endregion Constructors

        #region Methods

        public static Trigger Read(BinaryReader br)
        {
            Trigger t = new Trigger();
            t.Verts[0] = new WalkMesh.Vert
            {
                x = br.ReadInt16(),
                z = br.ReadInt16(),
                y = br.ReadInt16(),
            };
            t.Verts[1] = new WalkMesh.Vert
            {
                x = br.ReadInt16(),
                z = br.ReadInt16(),
                y = br.ReadInt16(),
            };
            t.DoorID = br.ReadByte();
            t.unknown = br.ReadBytes(3);
            return t;
        }

        #endregion Methods
    }
}