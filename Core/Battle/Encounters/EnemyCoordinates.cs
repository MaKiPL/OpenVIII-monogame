using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public class EnemyCoordinates : IReadOnlyList<Coordinate>
    {
        #region Fields

        private readonly IReadOnlyList<Coordinate> _enemies;

        #endregion Fields

        #region Constructors

        public EnemyCoordinates(IReadOnlyList<Coordinate> @in) => _enemies = @in;

        #endregion Constructors


        #region Methods

        public static implicit operator EnemyCoordinates(List<Coordinate> @in) => new EnemyCoordinates(@in);

        public static EnemyCoordinates Read(BinaryReader br)
        {
            return new EnemyCoordinates(Enumerable.Range(0, 8).Select(_ => Coordinate.Read(br)).ToList().AsReadOnly());
        }


        #endregion Methods

        public IEnumerator<Coordinate> GetEnumerator()
        {
            return _enemies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _enemies).GetEnumerator();
        }

        public int Count => _enemies.Count;

        public Coordinate this[int index] => _enemies[index];
    }
}