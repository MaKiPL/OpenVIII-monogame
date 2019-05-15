using System;
using System.Collections;
using System.Linq;

namespace FF8
{
    /// <summary>
    /// This holds the encoded bytes and provides implict casts to string and byte[]
    /// </summary>
    internal class FF8String : IEnumerator, IEnumerable
    {
        #region Fields

        private byte[] value;
        private int position = 0;

        #endregion Fields

        #region Constructors

        internal FF8String()
        {
        }

        internal FF8String(byte[] @value) => Value = @value;

        internal FF8String(string @value) => Value = Memory.DirtyEncoding.GetBytes(@value);

        #endregion Constructors

        #region Properties

        internal byte[] Value { get => value; set => this.value = value; }
        internal string Value_str => ToString();
        internal int Length => value==null? 0:value.Length;
        public object Current { get => Value[position-1] ; }

        #endregion Properties

        #region Indexers

        internal byte this[int index] => value[index];

        #endregion Indexers

        #region Methods

        public static implicit operator byte[] (FF8String input) => input?.Value;

        public static implicit operator FF8String(string input) => new FF8String(input);

        public static implicit operator FF8String(byte[] input) => new FF8String(input);

        public static implicit operator string(FF8String input) => input?.ToString();

        public override string ToString() => Memory.DirtyEncoding.GetString(Value);

        public bool MoveNext()
        {
            if(++position <= Length)
            return true;
            else
            {
                Reset();
                return false;
            }
        }

        public void Reset() => position = 0;
        public IEnumerator GetEnumerator() => this;
        internal FF8String Append(FF8String end)
        {
            if (end != null && end.Length > 0)
            {
                Array.Resize(ref value, Length + end.Length);
                Array.Copy(end, 0, value, Length, end.Length);
            }
            return this;
        }

        internal static FF8String Combine(FF8String start,FF8String end)
        {
            if (end != null && end.Length > 0)
            {
                byte[] combine = new byte[start.Length + end.Length];
                Array.Copy(start, 0, combine, 0, start.Length);
                Array.Copy(end, 0, combine, start.Length, end.Length);
                return combine;
            }
            else
            return start;
        }
        internal FF8String ReplaceRegion()
        {
            int i = 0;
            do
            {
                i = Array.FindIndex(value, i, Length - i, x => x == 0x0E);
                if (i >= 0)
                {
                    byte id = (byte)(value[i + 1] - 0x20);
                    //byte[] start = value.Take(i).ToArray();
                    byte[] newdata = Memory.Strings.Read(Strings.FileID.NAMEDIC, 0, id);
                    byte[] end = value.Skip(2 + i).ToArray();

                    Array.Resize(ref value, Length + newdata.Length - 2);
                    Array.Copy(newdata,0,value,i,newdata.Length);
                    Array.Copy(end, 0, value, i+newdata.Length, end.Length);
                    i+=newdata.Length;
                }
            }
            while (i >= 0&& i < Length);
            return this;
        }
        #endregion Methods
    }
}