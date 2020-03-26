using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Camera
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class Camera
    {
        #region Constructors

        private Camera(byte[] cab, long offset)
        {
            using (var br = new BinaryReader(new MemoryStream(cab)))
            {
                br.BaseStream.Seek(offset, SeekOrigin.Begin);
                const float multiple = 4096f;
                Axis = (
                    new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) / multiple,
                    new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) / multiple,
                    new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) / multiple);

                br.BaseStream.Seek(2, SeekOrigin.Current);
                Space = new Vector3(br.ReadInt32(), br.ReadInt32(), br.ReadInt32()) / multiple;
                Pan = (br.ReadInt16(), br.ReadInt16());
                Zoom = br.ReadUInt16();
                Position = -(Space * Axis.X + Space * Axis.Y + Space * Axis.X);
            }
        }

        #endregion Constructors

        #region Properties

        public (Vector3 X, Vector3 Y, Vector3 Z) Axis { get; }
        public (short X, short Y) Pan { get; }

        public Vector3 Position { get; }

        public Matrix RotationMatrix => new Matrix { Left = Axis.X, Down = Axis.Y, Backward = Axis.Z };

        public Vector3 Space { get; }

        public float Zoom { get; }

        #endregion Properties

        #region Methods

        public static Camera CreateInstance(byte[] idb, long offset) => new Camera(idb, offset);

        public Matrix CreateLookAt() => Matrix.CreateLookAt(Position - Axis.Z * 2, Axis.Z + Position, Axis.Y);

        public Matrix CreateWorld() => Matrix.CreateWorld(Position, Axis.Z, Axis.Y);

        #endregion Methods
    }
}