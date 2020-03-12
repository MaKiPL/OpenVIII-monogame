using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public struct DatFile
    {
        public static DatFile CreateInstance(BinaryReader br)
        {
            return new DatFile(br);
        }

        public readonly int CSections;
        public readonly uint[] PSections;
        public readonly uint Eof;

        private DatFile(BinaryReader br)
        {
            CSections = br.ReadInt32();
            PSections = Enumerable.Range(0, CSections).Select(_ => br.ReadUInt32()).ToArray();
            Eof = br.ReadUInt32();
        }
    }
}