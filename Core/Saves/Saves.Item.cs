namespace OpenVIII
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

            #region Properties

            public Item_In_Menu? DATA => Memory.MItems?.Items[ID];

            #endregion Properties
        };

        #endregion Structs

    }
}