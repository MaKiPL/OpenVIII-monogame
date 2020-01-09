using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.Fields
{
    public partial class WalkMesh
    {
        public class Access : ICloneable, IList<ushort>
        { 
            private List<ushort> items;

            public Access() => this.items = new List<ushort>(3);

            public ushort this[int index] { get => ((IList<ushort>)items)[index]; set => ((IList<ushort>)items)[index] = value; }

            public int Count => ((IList<ushort>)items).Count;

            public bool IsReadOnly => ((IList<ushort>)items).IsReadOnly;

            public void Add(ushort item) => ((IList<ushort>)items).Add(item);

            public void Clear() => ((IList<ushort>)items).Clear();

            public bool Contains(ushort item) => ((IList<ushort>)items).Contains(item);

            public void CopyTo(ushort[] array, int arrayIndex) => ((IList<ushort>)items).CopyTo(array, arrayIndex);

            public IEnumerator<ushort> GetEnumerator() => ((IList<ushort>)items).GetEnumerator();

            public int IndexOf(ushort item) => ((IList<ushort>)items).IndexOf(item);

            public void Insert(int index, ushort item) => ((IList<ushort>)items).Insert(index, item);

            public bool Remove(ushort item) => ((IList<ushort>)items).Remove(item);

            public void RemoveAt(int index) => ((IList<ushort>)items).RemoveAt(index);
            public bool IsWall(int index) => items[index] == 0xFFFF;
            IEnumerator IEnumerable.GetEnumerator() => ((IList<ushort>)items).GetEnumerator();
            public object Clone() => new Access { items = items.ToList()};
        }
    }
}