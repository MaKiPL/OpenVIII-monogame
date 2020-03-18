using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Sound IDs
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_SFX"/>
    public class SFX : IReadOnlyList<uint>
    {
        private readonly IReadOnlyList<uint> _sndIDs;
        public SFX(byte[] sfxB)
        {
            if (sfxB == null || sfxB.Length < 4) return;
            List<uint> sndIDs = new List<uint>(sfxB.Length / 4);
            MemoryStream ms;
            using (BinaryReader br = new BinaryReader(ms = new MemoryStream(sfxB)))
                while (ms.Position < ms.Length)
                    sndIDs.Add(br.ReadUInt32());
            _sndIDs = sndIDs.AsReadOnly();
        }

        public uint this[int index] => _sndIDs[index];

        public int Count => _sndIDs.Count;

        public IEnumerator<uint> GetEnumerator() => _sndIDs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _sndIDs.GetEnumerator();
    }
}