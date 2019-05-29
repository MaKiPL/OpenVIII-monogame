namespace FF8
{
    public static partial class Saves
    {
        #region Structs

        public struct Item
        {
            #region Fields

            public byte ID;
            public byte QTY;

            #endregion Fields

            #region Constructors

            public Item(byte iD, byte qTY) { ID = iD; QTY = qTY; }

            #endregion Constructors
        };

        #endregion Structs
    }
}