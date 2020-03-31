using System.Runtime.InteropServices;

namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class File
        {
            #region Structs

            [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
            public readonly struct Header
            {
                #region Fields

                [field: FieldOffset(0)]
                public readonly byte CountAreas;

                [field: FieldOffset(1)]
                public readonly byte CountDoors;

                [field: FieldOffset(2)]
                public readonly byte CountModules;

                [field: FieldOffset(3)]
                public readonly byte CountObjects;

                [field: FieldOffset(6)]
                public readonly ushort OperationsOffset;

                [field: FieldOffset(4)]
                public readonly ushort ScriptsOffset;

                #endregion Fields
            };

            #endregion Structs
        }

        #endregion Classes
    }
}