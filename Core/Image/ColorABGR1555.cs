using Microsoft.Xna.Framework;

namespace OpenVIII
{
    /// <summary>
    /// Read 16 bit color.
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/windows/win32/directshow/working-with-16-bit-rgb"/>
    public readonly struct ColorABGR1555
    {
        #region Fields

        /// <summary>
        /// blue bits
        /// </summary>
        private const ushort BlueMask = 0x7C00;

        /// <summary>
        /// green bits
        /// </summary>
        private const ushort GreenMask = 0x3E0;

        /// <summary>
        /// all bits except the STP bit.
        /// </summary>
        private const ushort NotSTPMask = 0x7FFF;

        /// <summary>
        /// red bits
        /// </summary>
        private const ushort RedMask = 0x1F;

        /// <summary>
        /// STP bit.
        /// </summary>
        private const ushort STPMask = 0x8000;

        #endregion Fields

        #region Constructors

        public ColorABGR1555(ushort value) => Value = value;

        public ColorABGR1555(byte r, byte g, byte b, byte a)
        {
            //could be wrong
            var stp = a > 0 && r + g + b == 0 || a == 0 && r + g + b > 0
                ? STPMask
                : (ushort)0;

            Value = (ushort)(((b >> 3) << 10) | ((g >> 3) << 5) | (r >> 3) | stp);
        }

        #endregion Constructors

        #region Properties

        public byte A => Value > 0 ? byte.MaxValue : byte.MinValue;
        public byte B => (byte)(((Value & BlueMask) >> 10) << 3);
        public byte G => (byte)(((Value & GreenMask) >> 5) << 3);
        public byte R => (byte)((Value & RedMask) << 3);
        public bool STP => (Value & NotSTPMask) != 0 && (Value & STPMask) != 0;
        public ushort Value { get; }

        #endregion Properties

        #region Methods

        public static implicit operator Color(ColorABGR1555 v) => new Color(v.R, v.G, v.B, v.A);

        public static implicit operator ColorABGR1555(Color v) => new ColorABGR1555(v.R, v.G, v.B, v.A);

        public static implicit operator ColorABGR1555(ushort v) => new ColorABGR1555(v);

        public static implicit operator ushort(ColorABGR1555 v) => v.Value;

        #endregion Methods
    }
}