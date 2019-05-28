using System;

namespace FF8.Framework
{
    public static class Rnd
    {
        [ThreadStatic] private static Random _rnd;

        public static Random Instance => _rnd ?? (_rnd = new Random());

        public static Int32 NextByte() => Instance.Next(0, Byte.MaxValue + 1);
    }
}