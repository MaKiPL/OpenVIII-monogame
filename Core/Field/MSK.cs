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
        private List<Vector3> CamPositions;

        public MSK(byte[] mskb)
        {
            if (mskb != null && mskb.Length > 4)
            {
                MemoryStream ms;
                using (BinaryReader br = new BinaryReader(ms = new MemoryStream(mskb)))
                {
                    int count = br.ReadInt32();
                    CamPositions = new List<Vector3>(count);
                    foreach (int i in Enumerable.Range(0, count))
                        CamPositions.Add(new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()));
                }
            }
        }

        public Vector3 this[int index] => ((IReadOnlyList<Vector3>)CamPositions)[index];

        public int Count => ((IReadOnlyList<Vector3>)CamPositions).Count;

        public IEnumerator<Vector3> GetEnumerator() => ((IReadOnlyList<Vector3>)CamPositions).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<Vector3>)CamPositions).GetEnumerator();
    }
}