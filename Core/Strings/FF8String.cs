using System;
using System.Collections;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// This holds the encoded bytes and provides implict casts to string and byte[]
    /// </summary>
    public class FF8String : IEnumerator, IEnumerable
    {
        #region Fields

        protected byte[] value;
        private static Encoding.FF8TextEncoding encoding = new Encoding.FF8TextEncoding(Encoding.FF8TextEncodingCodepage.Create());
        private int position = 0;

        #endregion Fields

        #region Constructors

        public FF8String()
        {
        }

        public FF8String(byte[] @value) => this.value = @value;

        public FF8String(string @value) => this.value = @value != null ? encoding.GetBytes(@value) : null;

        #endregion Constructors

        #region Properties

        public object Current => Value[position - 1];
        public virtual byte[] Value { get => value; set => this.value = value; }

        /// <summary>
        /// This is incorrect as null can be in the beginning of strings as a marker or tag.
        /// But I can't figure out how to make his code leave null as null.
        /// </summary>
        public string Value_str => encoding.GetString(Value ?? new byte[] { }).TrimEnd('\0').Replace("{End}", "");

        public virtual int Length => value == null ? 0 : value.Length;

        #endregion Properties

        #region Indexers

        public byte this[int index] => value[index];

        #endregion Indexers

        #region Methods

        public static FF8String Combine(FF8String start, FF8String end)
        {
            if (end != null && end.Length > 0)
            {
                var combine = new byte[start.Length + end.Length];
                Array.Copy(start, 0, combine, 0, start.Length);
                Array.Copy(end, 0, combine, start.Length, end.Length);
                return combine;
            }
            else
                return start;
        }

        public static implicit operator byte[] (FF8String input) => input?.Value;

        public static implicit operator FF8String(string input) => new FF8String(input);

        public static implicit operator FF8String(byte[] input) => new FF8String(input);

        public static implicit operator string(FF8String input) => input?.Value_str;

        public static FF8String operator +(FF8String a, FF8String b)
        {
            if (a != null && a.Length > 0)
            {
                var s = a.Clone();
                if (b != null && b.Length > 0)
                    return s.Append(b);
                else
                    return a;
            }
            return b;
        }

        public static FF8String operator +(FF8String a, string b)
        {
            if (a != null && a.Length > 0)
            {
                if (!string.IsNullOrEmpty(b))
                {
                    var s = a.Clone();
                    return s.Append(b);
                }
                else
                    return a;
            }
            return b;
        }

        public static FF8String operator +(string a, FF8String b)
        {
            var s = new FF8String(a);
            return s.Append(b);
        }

        public FF8String Append(FF8String end)
        {
            if (Value != null && Value.Length > 0)
            {
                if (end != null && end.Length > 0)
                {
                    Array.Resize(ref value, Length + end.Length);
                    Array.Copy(end.Value, 0, value, Length - end.Length, end.Length);
                }
            }
            else
            {
                if (end != null && end.Length > 0)
                {
                    Value = end;
                }
            }
            return this;
        }

        public FF8String Clone()
        {
            if (Length > 0)
                return new FF8String
                {
                    value = (byte[])(value.Clone())
                };
            else return new FF8String();
        }

        public FF8String Replace(FF8String a, FF8String b)
        {
            if (Length > 0 && a != null && b != null && a.Length > 0 && b.Length > 0)
            {
                var i = 0;
                do
                {
                    i = Array.FindIndex(value, i, Length - i, x => x == a[0]);
                    if (i >= 0)
                    {
                        if (value.Skip(i).Take(a.Length).ToArray().SequenceEqual(a.Value))
                        {
                            var end = value.Skip(a.Length + i).ToArray();

                            Array.Resize(ref value, Length + b.Length - a.Length);
                            Array.Copy(b, 0, value, i, b.Length);
                            Array.Copy(end, 0, value, i + b.Length, end.Length);
                            i += b.Length;
                        }
                        else
                        {
                            i++;
                        }
                    }
                    else break;
                }
                while (i < Length);
            }
            return this;
        }

        public IEnumerator GetEnumerator()
        {
            if (Value != null && Value.Length > 0)
                return this;
            return null;
        }

        public bool MoveNext()
        {
            if (++position <= Length)
                return true;
            else
            {
                Reset();
                return false;
            }
        }

        public void Reset() => position = 0;

        public override string ToString() => Value_str;

        #endregion Methods
    }
}