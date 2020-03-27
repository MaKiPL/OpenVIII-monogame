using System;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        public struct FieldObjectId
        {
            public int Value { get; }

            public FieldObjectId(int value)
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), $"Invalid character variable index: {value}");

                Value = value;
            }

            public override string ToString()
            {
                return $"character{Value}";
            }
        }
    }
}