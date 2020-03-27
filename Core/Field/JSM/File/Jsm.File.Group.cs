#pragma warning disable 649 // field is never assigned

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Group
            {
                private readonly ushort _value;

                public ushort Label => checked((ushort)(_value >> 7));
                public byte ScriptsCount => checked((byte)(_value & 0x7F));

                public override string ToString() => $"Label: {Label}, Script: {ScriptsCount}, Value: {_value}";
            }
        }
    }
}