namespace FF8
{

    public struct Loc
    {
        public uint seek;
        /// <summary>
        /// sometimes there is more than one entry at a location each is 8 bytes
        /// </summary>
        public uint length;
        public uint max => seek + length;

        public static bool operator !=(Loc a, Loc b) => a.seek != b.seek && a.length != b.length;
        public static bool operator ==(Loc a, Loc b) => a.seek == b.seek && a.length == b.length;

        public override bool Equals(object obj)
        {
            if (!(obj is Loc))
                return false;

            var loc = (Loc)obj;
            return seek == loc.seek &&
                   length == loc.length;
        }

        public static implicit operator uint(Loc @in) => @in.seek;

        public override int GetHashCode()
        {
            var hashCode = 202372718;
            hashCode = hashCode * -1521134295 + seek.GetHashCode();
            hashCode = hashCode * -1521134295 + length.GetHashCode();
            return hashCode;
        }
    }
}