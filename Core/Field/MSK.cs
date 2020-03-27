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
        #region Fields

        private readonly IReadOnlyList<Vector3> _camPositions;

        #endregion Fields

        #region Constructors

        public MSK(byte[] mskB)
        {
            if (mskB == null || mskB.Length <= 4) return;
            using (var br = new BinaryReader(new MemoryStream(mskB)))
                _camPositions = Enumerable.Range(0, br.ReadInt32())
                    .Select(_ => new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16())).ToList().AsReadOnly();
        }

        #endregion Constructors

        #region Properties

        public int Count => _camPositions.Count;

        #endregion Properties

        #region Indexers

        public Vector3 this[int index] => _camPositions[index];

        #endregion Indexers

        #region Methods

        public IEnumerator<Vector3> GetEnumerator() => _camPositions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _camPositions.GetEnumerator();

        #endregion Methods
    }
}