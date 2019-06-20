using System;
using System.Collections.Generic;

namespace FF8
{
    public sealed class OrderedDictionary<TKey, TValue>
    {
        private readonly List<TValue> _list;
        private readonly Dictionary<TKey, TValue> _dic;

        public OrderedDictionary()
        {
            _list = new List<TValue>();
            _dic = new Dictionary<TKey, TValue>();
        }

        public void Add(TKey key, TValue value)
        {
            _dic.Add(key, value);
            _list.Add(value);
        }

        public Boolean ContainsKey(TKey key)
        {
            return _dic.ContainsKey(key);
        }

        public Boolean ContainsIndex(Int32 index)
        {
            return index < _list.Count;
        }

        public Boolean TryGetByKey(TKey key, out TValue value)
        {
            return _dic.TryGetValue(key, out value);
        }

        public Boolean TryGetByIndex(Int32 index, out TValue value)
        {
            if (ContainsIndex(index))
            {
                value = _list[index];
                return true;
            }

            value = default(TValue);
            return false;
        }

        public IReadOnlyList<TValue> Values => _list;

        public void Clear()
        {
            _dic.Clear();
            _list.Clear();
        }
    }
}

