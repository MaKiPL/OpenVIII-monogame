using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public struct FieldObjectId
        {
            public Int32 Value { get; }

            public FieldObjectId(Int32 value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Invalid character variable index: {value}");

                Value = value;
            }

            public override String ToString()
            {
                return $"character{Value}";
            }
        }
    }
}