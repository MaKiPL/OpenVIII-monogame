using System;

#pragma warning disable 649 // field is never assigned

namespace FF8
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Operation
            {
                private readonly Int32 _value;

                public Jsm.Opcode Opcode
                {
                    get
                    {
                        if ((_value & 0xFF000000) != 0)
                            return (Jsm.Opcode)(_value >> 24);
                        return (Jsm.Opcode)_value;
                    }
                }

                public Int32 Parameter
                {
                    get
                    {
                        if ((_value & 0x00800000) == 0)
                            return (Int32)(_value & 0x00FFFFFF);
                        return (Int32)(_value | 0xFF000000);
                    }
                }
            }
        }
    }
}