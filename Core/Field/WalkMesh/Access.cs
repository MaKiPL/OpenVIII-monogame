using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Fields
{
    public partial class WalkMesh
    {
        public class Access : ICloneable, IList<short>
        { 
            private List<short> items;

            public Access() => this.items = new List<short>(3);

            public short this[int index] { get => ((IList<short>)items)[index]; set => ((IList<short>)items)[index] = value; }

            public int Count => ((IList<short>)items).Count;

            public bool IsReadOnly => ((IList<short>)items).IsReadOnly;

            public void Add(short item) => ((IList<short>)items).Add(item);

            public void Clear() => ((IList<short>)items).Clear();

            public bool Contains(short item) => ((IList<short>)items).Contains(item);

            public void CopyTo(short[] array, int arrayIndex) => ((IList<short>)items).CopyTo(array, arrayIndex);

            public IEnumerator<short> GetEnumerator() => ((IList<short>)items).GetEnumerator();

            public int IndexOf(short item) => ((IList<short>)items).IndexOf(item);

            public void Insert(int index, short item) => ((IList<short>)items).Insert(index, item);

            public bool Remove(short item) => ((IList<short>)items).Remove(item);

            public void RemoveAt(int index) => ((IList<short>)items).RemoveAt(index);
            /// <summary>
            /// Goal to label unpassable locations. Deling labeled as -1 unsure if it's -1 or lessthanequal
            /// </summary>
            /// <param name="index">Which side of triangle in clockwise order</param>
            /// <returns></returns>
            /// <see cref="https://github.com/myst6re/deling/blob/master/WalkmeshGLWidget.cpp"/>
            public bool IsWall(int index) => items[index] <= -1;
            IEnumerator IEnumerable.GetEnumerator() => ((IList<short>)items).GetEnumerator();
            public object Clone() => new Access { items = items.ToList()};
        }
    }
}