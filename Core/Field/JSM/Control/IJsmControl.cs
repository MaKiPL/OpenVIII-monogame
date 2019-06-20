using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Jsm
    {
        public interface IJsmControl
        {
            IEnumerable<Segment> EnumerateSegments();
        }
    }
}