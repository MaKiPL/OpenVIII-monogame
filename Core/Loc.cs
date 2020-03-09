// ReSharper disable NonReadonlyMemberInGetHashCode

namespace OpenVIII
{
    public struct Loc
    {
        #region Constructors

        private Loc(uint seek, uint length) => (Seek, Length) = (seek, length);

        #endregion Constructors

        #region Properties

        /// <summary>
        /// sometimes there is more than one entry at a location each is 8 bytes
        /// </summary>
        public uint Length { get; }

        public uint Max => Seek + Length;

        public uint Seek { get; }

        #endregion Properties

        #region Methods

        public static Loc CreateInstance(uint seek, uint length) => new Loc(seek, length);

        public static implicit operator Loc((uint seek, uint length) value) => new Loc(value.seek, value.length);

        public static implicit operator uint(Loc @input) => @input.Seek;

        public static bool operator !=(Loc a, Loc b) => a.Seek != b.Seek || a.Length != b.Length;

        public static bool operator ==(Loc a, Loc b) => a.Seek == b.Seek && a.Length == b.Length;

        public bool Equals(Loc other) => Seek == other.Seek && Length == other.Length;

        public override bool Equals(object obj) => obj is Loc other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Seek * 397) ^ (int)Length;
            }
        }

        #endregion Methods
    }
}