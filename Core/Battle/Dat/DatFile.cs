using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public struct DatFile
    {
        #region Fields

        public readonly int CSections;

        public readonly uint Eof;

        public readonly uint[] PSections;

        #endregion Fields

        #region Constructors

        private DatFile(BinaryReader br)
        {
            CSections = br.ReadInt32();
            PSections = Enumerable.Range(0, CSections).Select(_ => br.ReadUInt32()).ToArray();
            Eof = br.ReadUInt32();
        }

        #endregion Constructors

        #region Methods

        public static DatFile CreateInstance(BinaryReader br) => new DatFile(br);

        #endregion Methods
    }
}