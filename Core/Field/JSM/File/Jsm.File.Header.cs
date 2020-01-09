using System;

namespace OpenVIII.Fields
{
    public static partial class Jsm
    {
        public static partial class File
        {
            public struct Header
            {
                public Byte CountAreas;
                public Byte CountDoors;
                public Byte CountModules;
                public Byte CountObjects;
                public UInt16 ScriptsOffset;
                public UInt16 OperationsOffset;
            };
        }
    }
}