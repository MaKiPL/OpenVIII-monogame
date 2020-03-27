#pragma warning disable 649 // field is never assigned

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Script
            {
                private readonly ushort _value;

                public bool Flag => _value >> 15 != 0;
                public ushort Position => checked((ushort)(_value & 0x7FFF));
            }
        }
    }
}