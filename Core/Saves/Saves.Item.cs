using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenVIII
{
    public static partial class Saves
    {

        #region Structs

        [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 2)]
        public struct Item
        {

            #region Fields
            [FieldOffset(0)]
            public byte ID;
            [FieldOffset(1)]
            public byte QTY;

            #endregion Fields

            #region Constructors
            public Item(byte ID, byte QTY) {
                this.ID = ID;
                this.QTY = QTY;
            }

            public Item(KeyValuePair<byte, byte> e) : this(e.Key, e.Value)
            { }

            #endregion Constructors

            #region Properties

            public Item_In_Menu? DATA
            {
                get
                {
                    if (Memory.MItems?.Items != null && Memory.MItems.Items.Count > ID)
                        return Memory.MItems.Items[ID];
                    else return null;
                }
            }

            public override string ToString() => $"{{{nameof(ID)}: {DATA?.Name??ID.ToString()}, {nameof(QTY)}: {QTY}}}";
            public Item UsedOne()
            {
                if(QTY <= 1)
                {
                    ID = 0;
                }
                QTY--;
                return this;
            }
            public Item Remove()
            {
                QTY = 0;
                ID = 0;
                return this;
            }
            public Item Add(byte qty, byte? id = null)
            {
                ID = id ?? ID;
                if (ID > 0)
                {
                    byte Q = (byte)MathHelper.Clamp(qty + QTY, 0, 100);
                    if (Q > QTY)
                    {
                        QTY = Q;
                        
                    }
                }
                return this;
            }

            public Item Clone()
            {
                //shadowcopy
                Item d = new Item(ID, QTY);
                //deepcopy anything that needs it here.
                return d;
            }

            public Item Add(sbyte qty)
            {
                QTY = (byte)(QTY + qty);
                return this;
            }

            #endregion Properties
        };

        #endregion Structs

    }
}