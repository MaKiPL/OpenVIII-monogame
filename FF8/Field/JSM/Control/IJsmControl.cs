using System;
using System.Collections.Generic;

namespace FF8
{
    public static partial class Jsm
    {
        public interface IJsmControl
        {
            IEnumerable<Segment> EnumerateSegments();
        }
    }
}