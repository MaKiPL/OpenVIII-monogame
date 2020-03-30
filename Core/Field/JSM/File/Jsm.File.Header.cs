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
            public struct Header
            {
                #region Fields

                [field: FieldOffset(0)]
                public byte CountAreas;

                [field: FieldOffset(1)]
                public byte CountDoors;

                [field: FieldOffset(2)]
                public byte CountModules;

                [field: FieldOffset(3)]
                public byte CountObjects;

                [field: FieldOffset(6)]
                public ushort OperationsOffset;

                [field: FieldOffset(4)]
                public ushort ScriptsOffset;

                #endregion Fields
            };

            #endregion Structs
        }

        #endregion Classes
    }
}