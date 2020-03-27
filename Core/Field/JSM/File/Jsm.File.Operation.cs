#pragma warning disable 649 // field is never assigned

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Operation
            {
                private readonly int _value;

                public Jsm.Opcode Opcode
                {
                    get
                    {
                        if ((_value & 0xFF000000) != 0)
                            return (Jsm.Opcode)(_value >> 24);
                        return (Jsm.Opcode)_value;
                    }
                }

                public int Parameter
                {
                    get
                    {
                        if ((_value & 0x00800000) == 0)
                            return _value & 0x00FFFFFF;
                        return (int)(_value | 0xFF000000);
                    }
                }
            }
        }
    }
}