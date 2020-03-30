using System.Runtime.InteropServices;

#pragma warning disable 649 // field is never assigned

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class File
        {
            [StructLayout(LayoutKind.Explicit,Size = 4,Pack = 1)]
            public struct Operation
            {
                [field:FieldOffset(0)]
                private readonly int _value;

                public Opcode Opcode
                {
                    get
                    {
                        if ((_value & 0x_FF00_0000) != 0)
                            return (Opcode)(_value >> 24);
                        return (Opcode)_value;
                    }
                }

                public int Parameter
                {
                    get
                    {
                        if ((_value & 0x_0080_0000) == 0)
                            return _value & 0x_00FF_FFFF;
                        return (int)(_value | 0x_FF00_0000);
                    }
                }
            }
        }
    }
}