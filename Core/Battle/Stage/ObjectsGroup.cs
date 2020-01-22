using System.IO;

namespace OpenVIII.Battle
{
    public partial class Stage
    {
        #region Structs

        private struct ObjectsGroup
        {
            #region Fields

            public uint numberOfSections;
            public uint objectListPointer;
            public uint relativeEOF;
            public uint settings1Pointer;
            public uint settings2Pointer;

            #endregion Fields

            public static ObjectsGroup Read(uint pointer, BinaryReader br)
            {
                br.BaseStream.Seek(pointer, System.IO.SeekOrigin.Begin);
                return new ObjectsGroup()
                {
                    numberOfSections = br.ReadUInt32(),
                    settings1Pointer = pointer + br.ReadUInt32(),
                    objectListPointer = pointer + br.ReadUInt32(),
                    settings2Pointer = pointer + br.ReadUInt32(),
                    relativeEOF = pointer + br.ReadUInt32(),
                };
            }
        }

        #endregion Structs
    }
}