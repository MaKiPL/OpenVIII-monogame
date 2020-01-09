using System;
using System.Collections.Generic;

namespace OpenVIII.Fields
{
    public static partial class Jsm
    {
        public interface IJsmControl
        {
            IEnumerable<Segment> EnumerateSegments();
        }
    }
}