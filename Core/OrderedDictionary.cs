using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// <para>List of KeyValuePairs</para>
    /// <para>TKey cannot be int use a List</para>
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        #region Fields

        private List<KeyValuePair<TKey, TValue>> _list;

        #endregion Fields

        #region Constructors

        public OrderedDictionary(int capacity = 0) => _list = new List<KeyValuePair<TKey, TValue>>(capacity);

        public OrderedDictionary(IDictionary<TKey, TValue> copy) : this(copy.Count)
        {
            foreach (var pair in copy)
            {
                Add(new KeyValuePair<TKey, TValue>(pair.Key, pair.Value));
            }
        }

        #endregion Constructors

        #region Properties

        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public bool IsSynchronized => false;
        public IReadOnlyList<TKey> Keys => _list.Select(kvp => kvp.Key).ToList();
        public IReadOnlyList<TValue> Values => _list.Select(kvp => kvp.Value).ToList();
        public int Count => _list.Count;

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => _list.Select(kvp => kvp.Key).ToArray();

        ICollection<TValue> IDictionary<TKey, TValue>.Values => _list.Select(kvp => kvp.Value).ToArray();

        #endregion Properties

        #region Indexers

        public KeyValuePair<TKey, TValue> this[int index]
        {
            get
            {
                if (TryGetKVPByIndex(index, out var kvp))
                    return kvp;
                throw new KeyNotFoundException($"{this}::Index:{index} - Not Found!");
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetByKey(key, out var value))
                    return value;
                throw new KeyNotFoundException($"{this}::Key:{key} - Not Found!");
            }
            set
            {
                var index = _list.FindIndex(kvp => kvp.Key.Equals(key));
                if (index >= 0)
                    _list[index] = new KeyValuePair<TKey, TValue>(key, value);
                else
                    throw new KeyNotFoundException($"{this}::Key:{key} - Not Found!");
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }

        #endregion Indexers

        #region Methods

        public void Add(TKey key, TValue value)
        {
            if (!TryAdd(key, value)) throw new ArgumentException($"{this}::Key:{key} - Already Exists!");
        }

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear() => _list.Clear();

        public bool Contains(KeyValuePair<TKey, TValue> item) => _list.Contains(item);

        public Boolean ContainsIndex(Int32 index) => index < _list.Count;

        public Boolean ContainsKey(TKey key) => _list.Any(kvp => kvp.Key.Equals(key));

        public Boolean ContainsValue(TValue value) => _list.Any(kvp => kvp.Value.Equals(value));

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (Count <= array.Length + arrayIndex)
            {
                foreach (var i in _list)
                {
                    array[arrayIndex++] = i;
                }
                return;
            }
            throw new OverflowException($"{this}::Count:{Count} > array.Length:{array.Length} + arrayIndex:{arrayIndex}");
        }

        public IEnumerator GetEnumerator() => _list.GetEnumerator();

        public bool Remove(TKey key)
        {
            var kvp = _list.First(_kvp => _kvp.Key.Equals(key));
            return _list.Remove(kvp);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) => _list.Remove(item);

        public bool TryAdd(TKey key, TValue value)
        {
            if (!ContainsKey(key))
            {
                _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                return true;
            }
            return false;
        }

        public Boolean TryGetByIndex(Int32 index, out TValue value)
        {
            if (ContainsIndex(index))
            {
                value = _list[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public Boolean TryGetByKey(TKey key, out TValue value)
        {
            if (ContainsKey(key))
            {
                value = _list.First(kvp => kvp.Key.Equals(key)).Value;
                return true;
            }
            value = default;
            return false;
        }

        public Boolean TryGetIndexByKey(TKey key, out int value)
        {
            if (ContainsKey(key))
            {
                value = _list.FindIndex(kvp => kvp.Key.Equals(key));
                return true;
            }
            value = -1;
            return false;
        }

        public Boolean TryGetKeyByIndex(Int32 index, out TKey value)
        {
            if (ContainsIndex(index))
            {
                value = _list[index].Key;
                return true;
            }

            value = default;
            return false;
        }

        public Boolean TryGetKVPByIndex(Int32 index, out KeyValuePair<TKey, TValue> value)
        {
            if (ContainsIndex(index))
            {
                value = _list[index];
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => TryGetByKey(key, out value);

        public OrderedDictionary<TKey, TValue> Clone() => new OrderedDictionary<TKey, TValue>(this);

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => _list.GetEnumerator();

        #endregion Methods
    }
}