using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FF8
{
    /// <summary>
    /// This holds the encoded bytes and provides implict casts to string and byte[]
    /// </summary>
    public class FF8String : IEnumerator, IEnumerable, ICloneable
    {
        #region Fields
        
        private byte[] value;
        private int position = 0;

        #endregion Fields

        #region Constructors

        public FF8String()
        {
        }

        public static void Init()
        {
            if (BytetoStr == null)
            {
                BytetoStr = new Dictionary<byte, FF8String>
                {
                    //{0x01, "" },
                    {0xC6, "VI"},// pos:166, col:20, row:9 --
                    {0xC7, "II"},// pos:167, col:21, row:9 --
                    //pc version sysfld00 and 01
                    {0xCC, "GA"},// pos:172, col:5, row:9 --
                    {0xCD, "ME"},// pos:173, col:6, row:9 --
                    {0xCE, "FO"},// pos:174, col:7, row:9 --
                    {0xCF, "LD"},// pos:175, col:8, row:9 --
                    {0xD0, "ER"},// pos:176, col:9, row:9 --
                    ////original texture - sysfont
                    //{0xCC, "ME"},// pos:172, col:5, row:9 --
                    //{0xCD, "MO"},// pos:173, col:6, row:9 --
                    //{0xCE, "RY"},// pos:174, col:7, row:9 --
                    //{0xCF, "CA"},// pos:175, col:8, row:9 --
                    //{0xD0, "RD"},// pos:176, col:9, row:9 --

                    {0xD1, "Sl"},// pos:177, col:10, row:9 --
                    {0xD2, "ot"},// pos:178, col:11, row:9 --
                    {0xD3, "ing"},// pos:179, col:12, row:10 --
                    {0xD4, "St"},// pos:180, col:13, row:10 --
                    {0xD5, "ec"},// pos:181, col:14, row:10 --
                    {0xD6, "kp"},// pos:182, col:15, row:10 --
                    {0xD7, "la"},// pos:183, col:16, row:10 --
                    {0xD8, ":z"},// pos:184, col:17, row:10 --
                    {0xD9, "Fr"},// pos:185, col:18, row:10 --
                    {0xDA, "nt"},// pos:186, col:19, row:10 --
                    {0xDB, "elng"},// pos:187, col:20, row:10 --
                    {0xDC, "re"},// pos:188, col:21, row:10 --
                    {0xDD, "S:"},// pos:189, col:1, row:10 --
                    {0xDE, "so"},// pos:190, col:2, row:10 --
                    {0xDF, "Ra"},// pos:191, col:3, row:10 --
                    {0xE0, "nu"},// pos:192, col:4, row:10 --
                    {0xE1, "ra"},// pos:193, col:5, row:10 --
                    //{0xE3, ""},// pos:195, col:0, row:0 --
                    //{0xE4, ""},// pos:196, col:0, row:0 --
                    //{0xE5, ""},// pos:197, col:0, row:0 --
                    //{0xE6, ""},// pos:198, col:0, row:0 --
                    //{0xE7, ""},// pos:199, col:0, row:0 --
                    {0xE8, "in"},// pos:200, col:0, row:0 --
                    {0xE9, "e "},// pos:201, col:0, row:0 --
                    {0xEA, "ne"},// pos:202, col:0, row:0 --
                    {0xEB, "to"},// pos:203, col:0, row:0 --
                    {0xEC, "re"},// pos:204, col:0, row:0 --
                    {0xED, "HP"},// pos:205, col:0, row:0 --
                    {0xEE, "l "},// pos:206, col:0, row:0 --
                    {0xEF, "ll"},// pos:207, col:0, row:0 --
                    {0xF0, "GF"},// pos:208, col:0, row:0 --
                    {0xF1, "nt"},// pos:209, col:0, row:0 --
                    {0xF2, "il"},// pos:210, col:0, row:0 --
                    {0xF3, "o "},// pos:211, col:0, row:0 --
                    {0xF4, "ef"},// pos:212, col:0, row:0 --
                    {0xF5, "on"},// pos:213, col:0, row:0 --
                    {0xF6, " w"},// pos:214, col:0, row:0 --
                    {0xF7, " r"},// pos:215, col:0, row:0 --
                    {0xF8, "wi"},// pos:216, col:0, row:0 --
                    {0xF9, "fi"},// pos:217, col:0, row:0 --
                    //{0xFA, ""},// pos:218, col:0, row:0 --
                    {0xFB, "s "},// pos:219, col:0, row:0 --
                    {0xFC, "ar"},// pos:220, col:0, row:0 --
                    //{0xFD, ""},// pos:221, col:0, row:0 --
                    {0xFE, " S"},// pos:222, col:0, row:0 --
                    {0xFF, "ag"},// pos:223, col:0, row:0 --
                };
            }
        }
        /// <summary>
        /// multi characters bytes and double character bytes
        /// </summary>
        /// TODO replace me.
        public static Dictionary<byte, FF8String> BytetoStr { get; private set; }

        public FF8String(byte[] @value) => Value = @value;
        private static Encoding.FF8TextEncoding encoding = new Encoding.FF8TextEncoding(Encoding.FF8TextEncodingCodepage.Create());
        public FF8String(string @value) => Value = encoding.GetBytes(@value);

        #endregion Constructors

        #region Properties

        public byte[] Value { get => value; set => this.value = value; }
        public string Value_str => ToString();
        public int Length => value==null? 0:value.Length;
        public object Current { get => Value[position-1] ; }

        #endregion Properties

        #region Indexers

        public byte this[int index] => value[index];

        #endregion Indexers

        #region Methods

        public static implicit operator byte[] (FF8String input) => input?.Value;

        public static implicit operator FF8String(string input) => new FF8String(input);

        public static implicit operator FF8String(byte[] input) => new FF8String(input);

        public static implicit operator string(FF8String input) => input?.ToString();

        public override string ToString() => encoding.GetString(Value).TrimEnd('\0');

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
        public FF8String Append(FF8String end)
        {
            if (end != null && end.Length > 0)
            {
                Array.Resize(ref value, Length + end.Length);
                Array.Copy(end, 0, value, Length, end.Length);
            }
            return this;
        }

        public static FF8String Combine(FF8String start,FF8String end)
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
        public FF8String ReplaceRegion()
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

        public object Clone() => new FF8String
        {
            //position = position,
            value = value
        };
        #endregion Methods
    }
}