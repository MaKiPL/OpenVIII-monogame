namespace OpenVIII.Fields.Scripts
{
    public static partial class Jsm
    {
        #region Classes

        public static partial class File
        {
            #region Structs

            public struct Header
            {
                #region Fields

                public byte CountAreas;
                public byte CountDoors;
                public byte CountModules;
                public byte CountObjects;
                public ushort OperationsOffset;
                public ushort ScriptsOffset;

                #endregion Fields
            };

            #endregion Structs
        }

        #endregion Classes
    }
}