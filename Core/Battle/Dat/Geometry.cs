using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace OpenVIII.Battle.Dat
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class Geometry
    {
        #region Fields

        /// <summary>
        /// Number of objects
        /// </summary>
        public readonly int CObjects;

        /// <summary>
        /// Object Data (see below)
        /// </summary>
        public readonly IReadOnlyList<Object> Objects;

        /// <summary>
        /// Object Positions
        /// </summary>
        public readonly IReadOnlyList<uint> PObjects;

        /// <summary>
        /// Total count of vertices
        /// </summary>
        public readonly uint CTotalVertices;

        #endregion Fields

        #region Constructors

        private Geometry(BinaryReader br, long byteOffset)
        {
            CObjects = br.ReadInt32();
            PObjects = Enumerable.Range(0, CObjects).Select(_ => br.ReadUInt32()).ToList().AsReadOnly();
            Objects = PObjects.Select(pOffset => Object.CreateInstance(br, pOffset + byteOffset)).ToList().AsReadOnly();
            CTotalVertices = br.ReadUInt32();
        }

        #endregion Constructors

        #region Methods

        public static Geometry CreateInstance(BinaryReader br, long byteOffset)
        {
            br.BaseStream.Seek(byteOffset, SeekOrigin.Begin);
            return new Geometry(br, byteOffset);
        }

        #endregion Methods
    }
}