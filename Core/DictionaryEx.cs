using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// class to add function to dictionary
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/22595655/how-to-do-a-dictionary-reverse-lookup"/>
    /// <seealso cref="https://stackoverflow.com/questions/6050633/why-doesnt-dictionary-have-addrange"/>
    public static class DictionaryEx
    {
        #region Methods

        public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dic, Dictionary<TKey, TValue> dicToAdd) => dicToAdd.ForEach(x => dic.Add(x.Key, x.Value));

        public static void AddRangeNewOnly<TKey, TValue>(this Dictionary<TKey, TValue> dic, Dictionary<TKey, TValue> dicToAdd) => dicToAdd.ForEach(x => { if (!dic.ContainsKey(x.Key)) dic.Add(x.Key, x.Value); });

        public static void AddRangeOverride<TKey, TValue>(this Dictionary<TKey, TValue> dic, Dictionary<TKey, TValue> dicToAdd) => dicToAdd.ForEach(x => dic[x.Key] = x.Value);

        public static bool ContainsKeys<TKey, TValue>(this Dictionary<TKey, TValue> dic, IEnumerable<TKey> keys)
        {
            bool result = false;
            keys.ForEachOrBreak((x) => { result = dic.ContainsKey(x); return result; });
            return result;
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        public static void ForEachOrBreak<T>(this IEnumerable<T> source, Func<T, bool> func)
        {
            foreach (T item in source)
            {
                bool result = func(item);
                if (result) break;
            }
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <see cref="https://stackoverflow.com/questions/2229951/how-do-i-get-a-key-from-a-ordereddictionary-in-c-sharp-by-index"/>
        public static T GetKey<T>(this OrderedDictionary dictionary, int index)
        {
            if (dictionary == null)
            {
                return default;
            }

            try
            {
                return (T)dictionary.Cast<DictionaryEntry>().ElementAt(index).Key;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Get Value from ordered dictionary
        /// </summary>
        /// <typeparam name="T">Key type</typeparam>
        /// <typeparam name="U">Value type</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <see cref="https://stackoverflow.com/questions/2229951/how-do-i-get-a-key-from-a-ordereddictionary-in-c-sharp-by-index"/>
        public static U GetValue<T, U>(this OrderedDictionary dictionary, T key)
        {
            if (dictionary == null)
            {
                return default;
            }

            try
            {
                return (U)dictionary.Cast<DictionaryEntry>().AsQueryable().Single(kvp => ((T)kvp.Key).Equals(key)).Value;
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <summary>
        /// Reverses Key and Value of dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Dictionary<TValue, TKey> Reverse<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            Dictionary<TValue, TKey> dictionary = new Dictionary<TValue, TKey>();
            foreach (KeyValuePair<TKey, TValue> entry in source)
            {
                if (!dictionary.ContainsKey(entry.Value))
                    dictionary.Add(entry.Value, entry.Key);
            }
            return dictionary;
        }

        public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        => new Vector3(v.X, v.Y, z);
        #endregion Methods
    }

    //public class _OrderedDictionay<T, U> : OrderedDictionary
    //{
    //    public _OrderedDictionay(int capacity) : base(capacity)
    //    {
    //    }
    //    #region Indexers

    //    public new KeyValuePair<T, U> this[int index]
    //    {
    //        get
    //        {
    //            T key = this.GetKey<T>(index);
    //            U val = this.GetValue<T, U>(key);
    //            return new KeyValuePair<T, U>(key, val);
    //        }
    //    }
    //    public U this[T key] => this.GetValue<T, U>(key);

    //    #endregion Indexers
    //}
}