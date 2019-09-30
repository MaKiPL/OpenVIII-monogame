using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class TTCardInfo
    {
        #region Fields

        private const byte mask = 0x7F;

        #endregion Fields

        #region Constructors

        public TTCardInfo()
        {
            Unlocked = false;
            Qty = 0;
            Location = 0;
        }

        public TTCardInfo(byte raw,byte location = 0)
        {
            Unlocked = ((raw >> 7) != 0);
            Qty = (byte)MathHelper.Clamp(raw & mask, 0, 100);
            Location = location;
        }

        public TTCardInfo(bool unlocked, byte qty, byte location = 0)
        {
            Unlocked = unlocked;
            Qty = (byte)MathHelper.Clamp(qty & mask, 0, 100);
            Location = location;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Location is really for the last 33 cards that are unique. It's set to which npc has the card.
        /// </summary>
        public byte Location { get; set; }

        /// <summary>
        /// The last 33 cards only have 1 copy. Rest can have up to 100.
        /// </summary>
        public byte Qty { get; set; }

        /// <summary>
        /// Unlocked is 1st bit of qty. If set the menu will show the card info. Else the game thinks
        /// you don't know about the card.
        /// </summary>
        public bool Unlocked { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Create a copy of this object.
        /// </summary>
        /// <returns></returns>
        public TTCardInfo Clone() => (TTCardInfo)MemberwiseClone();

        #endregion Methods
    }
}