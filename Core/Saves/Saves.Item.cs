using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

            public Item(byte id, byte qty)
            {
                ID = id;
                QTY = qty;
            }

            public Item(KeyValuePair<byte, byte> e) : this(e.Key, e.Value)
            { }

            #endregion Constructors

            #region Properties

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public ItemInMenu? Data
            {
                get
                {
                    if (Memory.MItems?.Items != null && Memory.MItems.Items.Count > ID)
                        return Memory.MItems.Items[ID];
                    return null;
                }
            }

            #endregion Properties

            #region Methods

            public Item Add(byte qty, byte? id = null)
            {
                ID = id ?? ID;
                if (ID <= 0) return this;
                var q = (byte)MathHelper.Clamp(qty + QTY, 0, 100);
                if (q > QTY)
                {
                    QTY = q;
                }
                return this;
            }

            public Item Add(sbyte qty)
            {
                QTY = (byte)(QTY + qty);
                return this;
            }

            public Item Clone()
            {
                //shadow copy
                var d = new Item(ID, QTY);
                //deep copy anything that needs it here.
                return d;
            }

            public Item Remove()
            {
                QTY = 0;
                ID = 0;
                return this;
            }

            public override string ToString() => $"{{{nameof(ID)}: {Data?.Name ?? ID.ToString()}, {nameof(QTY)}: {QTY}}}";

            public Item UsedOne()
            {
                if (QTY <= 1)
                {
                    ID = 0;
                }
                QTY--;
                return this;
            }

            #endregion Methods
        };

        #endregion Structs
    }
}