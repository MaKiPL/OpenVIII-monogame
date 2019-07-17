using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        public partial class VictoryMenu : Menu
        {
            public enum Mode
            {
                Exp,
                Items,
                AP,
                All,
            }

            protected override void Init()
            {
                Size = new Vector2(881, 606);
                Data = new Dictionary<Enum, IGMData>
                {
                    { Mode.All, new IGMData_Group(
                    new IGMData_Container(new IGMDataItem_Box(new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.Green,
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm,
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.White})+" "+(Memory.Strings.Read(Strings.FileID.KERNEL,30,22)),new Rectangle(new Point(0,(int)Size.Y-78),new Point((int)Size.X,78)),options: Box_Options.Center| Box_Options.Middle))
                    )},
                    { Mode.Exp,
                    new IGMData_PlayerEXPGroup (
                        new IGMData_PlayerEXP(_exp,0),new IGMData_PlayerEXP(_exp,1),new IGMData_PlayerEXP(_exp,2)
                        )
                    { CONTAINER = new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))} },
                    { Mode.Items,
                    new IGMData_PartyItems(_items,new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))) },

                };
                SetMode(Mode.Items);
                InputFunctions = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.Exp, Data[Mode.Exp].Inputs},
                    { Mode.Items, Data[Mode.Items].Inputs}
                };
                base.Init();
            }
            public override bool SetMode(Enum mode)
            {
                switch ((Mode)mode)
                {
                    case Mode.Exp:
                        Data[Mode.Exp].Show();
                        Data[Mode.Items].Hide();
                        break;
                    case Mode.Items:
                        Data[Mode.Exp].Hide();
                        Data[Mode.Items].Show();
                        break;
                }
                return base.SetMode((Mode)mode);
            }

            public override bool Inputs()
            {
                if (InputFunctions != null && InputFunctions.ContainsKey((Mode)GetMode()))
                    return InputFunctions[(Mode)GetMode()]();
                return false;
            }

            private int _ap = 0;
            private int _exp = 0;
            private Saves.Item[] _items = null;

            /// <summary>
            /// if you use this you will get no exp, ap, or items
            /// </summary>
            public override void ReInit() { }

            /// <summary>
            /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
            /// </summary>
            public override void ReInit(Characters c, Characters vc, bool backup = false) { }

            public void ReInit(int exp, int ap, params Saves.Item[] items)
            {
                _exp = exp;
                ((IGMData_PlayerEXPGroup)Data[Mode.Exp]).EXP = _exp;
                _ap = ap;
                _items = items;
                ((IGMData_PartyItems)Data[Mode.Items]).Items = _items;
                base.ReInit();
            }

            /// <summary>
            /// <para>EXP Acquired</para>
            /// <para>Current EXP</para>
            /// <para>Next LEVEL</para>
            /// </summary>
            private static FF8String ECN;

            private Dictionary<Mode, Func<bool>> InputFunctions;

            private class IGMData_PartyItems : IGMData
            {
                private Saves.Item[] _items;
                private int _item;

                public int Item
                {
                    get => _item; protected set
                    {
                        if (_items != null&&  value < _items.Length)
                            _item = value;
                        else
                        {
                            _items = null;
                            _item = 0;
                        }
                    }
                }

                public Saves.Item[] Items
                {
                    get => _items; set
                    {
                        _items = value;
                        _item = 0;
                    }
                }

                public IGMData_PartyItems(Saves.Item[] items, IGMDataItem container) : base()
                {
                    _items = items;
                    Init(1, 7, container, 1, 1);
                }

                protected override void Init()
                {
                    base.Init();
                    ITEM[0, 0] = new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 21), new Rectangle(SIZE[0].X, SIZE[0].Y, SIZE[0].Width, 78), Icons.ID.INFO, options: Box_Options.Middle);
                    ITEM[0, 1] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X + 140, SIZE[0].Y + 189, 475, 78), Icons.ID.ITEM, options: Box_Options.Middle); // item name
                    ITEM[0, 2] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X + 615, SIZE[0].Y + 189, 125, 78), Icons.ID.NUM_, options: Box_Options.Middle | Box_Options.Center); // item count
                    ITEM[0, 3] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X, SIZE[0].Y + 444, SIZE[0].Width, 78), Icons.ID.HELP, options: Box_Options.Middle); // item description
                    ITEM[0, 4] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 28),SIZE[0].X+232,SIZE[0].Y+315, Icons.ID.NOTICE,Box_Options.Center, SIZE[0])); // Couldn't find any items 
                    ITEM[0, 5] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 24), SIZE[0].X + 230, SIZE[0].Y + 291, Icons.ID.NOTICE, Box_Options.Center, SIZE[0])); // over 100 discarded
                    ITEM[0, 6] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 6), SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center, SIZE[0])); // Recieved item
                    Item = 0;
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }
                public override void ReInit()
                {
                    base.ReInit();
                    ((IGMDataItem_Box)ITEM[0, 1]).Data = _items?[_item].DATA?.Name;
                    ((IGMDataItem_Box)ITEM[0, 2]).Data = $"{_items?[_item].QTY}";
                    ((IGMDataItem_Box)ITEM[0, 3]).Data = _items?[_item].DATA?.Description;
                    ITEM[0, 4].Hide();
                    ITEM[0, 5].Hide();
                    ITEM[0, 6].Hide();
                }
                public override void Inputs_OKAY()
                {
                    Item++;
                    ReInit();
                    base.Inputs_OKAY();
                }
            }
        }
    }
}