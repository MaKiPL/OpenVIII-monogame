using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = ByteSize)]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class Vertex
    {
        #region Fields

        private const int ByteSize = 6;

        #endregion Fields

        #region Constructors

        private Vertex(short x, short y, short z)
        => (X, Y, Z) = (x, y, z);

        private Vertex(BinaryReader br)
            => (X, Y, Z) = (br.ReadInt16(), br.ReadInt16(), br.ReadInt16());

        private Vertex()
        {
        }

        #endregion Constructors

        #region Properties

        [field: FieldOffset(0)]
        public short X { get; }

        [field: FieldOffset(2)]
        public short Y { get; }

        [field: FieldOffset(4)]
        public short Z { get; }

        #endregion Properties

        #region Methods

        public static Vertex CreateInstance(short x, short y, short z) => new Vertex(x, y, z);

        public static Vertex CreateInstance(BinaryReader br) => Extended.ByteArrayToClass<Vertex>(br.ReadBytes(ByteSize));

        public static IReadOnlyList<Vector3> CreateInstances(BinaryReader br, ushort count) => Enumerable
            .Range(0, count).Select(_ => (Vector3)CreateInstance(br)).ToList().AsReadOnly();

        public static implicit operator Vector3(Vertex v) => new Vector3(-v.X / DebugBattleDat.ScaleHelper,
            -v.Z / DebugBattleDat.ScaleHelper, -v.Y / DebugBattleDat.ScaleHelper);

        public override string ToString() =>
            $"{nameof(X)}={X}, {nameof(Y)}={Y}, {nameof(Z)}={Z}, {nameof(Vector3)}={(Vector3)this}";

        #endregion Methods
    }
}