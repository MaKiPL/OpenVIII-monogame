using System;

#pragma warning disable 649 // field is never assigned

namespace OpenVIII
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Group
            {
                private readonly UInt16 _value;

                public UInt16 Label => checked((UInt16)(_value >> 7));
                public Byte ScriptsCount => checked((Byte)(_value & 0x7F));

                public override string ToString()
                {
                    return $"Label: {Label}, Script: {ScriptsCount}, Value: {_value}";
                }
            }
        }
    }
}