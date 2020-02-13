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
        List<uint> sndids;
        public SFX(byte[] sfxb)
        {
            if (sfxb != null && sfxb.Length >= 4)
            {
                sndids = new List<uint>(sfxb.Length / 4);
                MemoryStream ms;
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(sfxb)))
                    while (ms.Position < ms.Length)
                        sndids.Add(br.ReadUInt32());
            }
        }

        public uint this[int index] => ((IReadOnlyList<uint>)sndids)[index];

        public int Count => ((IReadOnlyList<uint>)sndids).Count;

        public IEnumerator<uint> GetEnumerator() => ((IReadOnlyList<uint>)sndids).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<uint>)sndids).GetEnumerator();
    }
}