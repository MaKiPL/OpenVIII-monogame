using System;

#pragma warning disable 649 // field is never assigned

namespace FF8
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Script
            {
                private readonly UInt16 _value;

                public Boolean Flag => _value >> 15 != 0;
                public UInt16 Position => checked((UInt16)(_value & 0x7FFF));
            }
        }
    }
}