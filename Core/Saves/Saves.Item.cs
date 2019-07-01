using System;

namespace OpenVIII
{
    public static partial class Saves
    {

        #region Structs

        public class Item
        {

            #region Fields

            public byte ID;
            public byte QTY;

            #endregion Fields

            #region Constructors

            public Item(byte ID, byte QTY) { this.ID = ID; this.QTY = QTY; }

            #endregion Constructors

            #region Properties

            public Item_In_Menu? DATA => Memory.MItems?.Items[ID];

            public void UsedOne()
            {
                if(QTY <= 1)
                {
                    ID = 0;
                }
                QTY--;
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

            public Item Clone()
            {
                //shadowcopy
                Item d = new Item(ID, QTY);
                //deepcopy anything that needs it here.
                return d;
            }
            #endregion Properties
        };

        #endregion Structs

    }
}