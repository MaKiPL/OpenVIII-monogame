namespace OpenVIII
{
    public static partial class Saves
    {
        public struct CompatibilitywithGF
        {
            private ushort value;
            public ushort Value => value;
            public static implicit operator CompatibilitywithGF(ushort d) => new CompatibilitywithGF {value = d};
            public static implicit operator ushort(CompatibilitywithGF d) => d.Value;
            public ushort Display => checked((ushort)((6000 - value) / 5));
            public override string ToString() => Display.ToString();
        }
    }
}