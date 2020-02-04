using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle
{
    public class EnemyCoordinates : IList<Coordinate>, ICollection<Coordinate>, IEnumerable<Coordinate>, IEnumerable, ICollection, IReadOnlyList<Coordinate>, IReadOnlyCollection<Coordinate>
    {
        #region Fields

        private List<Coordinate> enemies;

        #endregion Fields

        #region Constructors

        public EnemyCoordinates(List<Coordinate> @in) => enemies = @in;

        public EnemyCoordinates() => enemies = new List<Coordinate>(8);

        #endregion Constructors

        #region Properties

        public int Count => ((ICollection<Coordinate>)enemies).Count;

        public bool IsReadOnly => ((ICollection<Coordinate>)enemies).IsReadOnly;

        public bool IsSynchronized => ((ICollection)enemies).IsSynchronized;

        public object SyncRoot => ((ICollection)enemies).SyncRoot;

        #endregion Properties

        #region Indexers

        public Coordinate this[int index] => ((IReadOnlyList<Coordinate>)enemies)[index];

        Coordinate IList<Coordinate>.this[int index] { get => ((IList<Coordinate>)enemies)[index]; set => ((IList<Coordinate>)enemies)[index] = value; }

        #endregion Indexers

        #region Methods

        public static implicit operator EnemyCoordinates(List<Coordinate> @in) => new EnemyCoordinates(@in);

        public static EnemyCoordinates Read(BinaryReader br)
        {
            EnemyCoordinates ec = new EnemyCoordinates();
            foreach (int i in Enumerable.Range(0, ec.enemies.Capacity))
            {
                Coordinate item = Coordinate.Read(br);
                ec.Add(item);
            }
            return ec;
        }

        public void Add(Coordinate item) => ((ICollection<Coordinate>)enemies).Add(item);

        public void Clear() => ((ICollection<Coordinate>)enemies).Clear();

        public bool Contains(Coordinate item) => ((ICollection<Coordinate>)enemies).Contains(item);

        public void CopyTo(Coordinate[] array, int arrayIndex) => ((ICollection<Coordinate>)enemies).CopyTo(array, arrayIndex);

        public void CopyTo(Array array, int index) => ((ICollection)enemies).CopyTo(array, index);

        public IEnumerator<Coordinate> GetEnumerator() => ((ICollection<Coordinate>)enemies).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((ICollection<Coordinate>)enemies).GetEnumerator();

        public int IndexOf(Coordinate item) => ((IList<Coordinate>)enemies).IndexOf(item);

        public void Insert(int index, Coordinate item) => ((IList<Coordinate>)enemies).Insert(index, item);

        public bool Remove(Coordinate item) => ((ICollection<Coordinate>)enemies).Remove(item);

        public void RemoveAt(int index) => ((IList<Coordinate>)enemies).RemoveAt(index);

        #endregion Methods
    }
}