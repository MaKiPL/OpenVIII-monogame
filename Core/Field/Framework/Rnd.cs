using System;

namespace OpenVIII.Fields
{
    public static class Rnd
    {
        [ThreadStatic] private static Random _rnd;

        public static Random Instance => _rnd ?? (_rnd = Memory.Random);

        public static Int32 NextByte() => Instance.Next(0, Byte.MaxValue + 1);
    }
}