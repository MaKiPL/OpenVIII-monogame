using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 8)]
    public struct Vertex
    {
        public short X { get; }
        public short Y { get; }
        public short Z { get; }

        public Vertex(short x, short y, short z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static implicit operator Vector3(Vertex v) => new Vector3(-v.X / DebugBattleDat.ScaleHelper, -v.Z / DebugBattleDat.ScaleHelper, -v.Y / DebugBattleDat.ScaleHelper);

        public override string ToString() => $"x={X}, y={Y}, z={Z}, Vector3={(Vector3)this}";
    }
}