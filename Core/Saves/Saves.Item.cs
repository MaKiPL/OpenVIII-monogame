using System;

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

            public void UsedOne()
            {
                if(QTY <= 1)
                {
                    ID = 0;
                }
                QTY -= 1;
            }
            public void Remove()
            {
                QTY = 0;
                ID = 0;
            }
            public void Add(byte qty, byte? id = null)
            {
                ID = id ?? ID;
                if (ID > 0)
                {
                    int x = QTY + qty;
                    if (x > 100) x = 100;
                    QTY = (byte)x;
                }
            }
            #endregion Properties
        };

        #endregion Structs

    }
}