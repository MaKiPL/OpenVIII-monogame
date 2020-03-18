using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Movie Cam?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/FileFormat_MSK"/>
    public class MSK : IReadOnlyList<Vector3>
    {
        private readonly IReadOnlyList<Vector3> _camPositions;

        public MSK(byte[] mskB)
        {
            if (mskB == null || mskB.Length <= 4) return;
            using (BinaryReader br = new BinaryReader(new MemoryStream(mskB)))
                _camPositions = Enumerable.Range(0, br.ReadInt32())
                    .Select(_ => new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16())).ToList().AsReadOnly();
            
        }

        public Vector3 this[int index] => _camPositions[index];

        public int Count => _camPositions.Count;

        public IEnumerator<Vector3> GetEnumerator() => _camPositions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _camPositions.GetEnumerator();
    }
}