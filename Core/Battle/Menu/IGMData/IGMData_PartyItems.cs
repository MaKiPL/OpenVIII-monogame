using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        public partial class VictoryMenu
        {
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
                public override void Refresh()
                {
                    base.Refresh();
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
                    Refresh();
                    base.Inputs_OKAY();
                }
            }
        }
    }
}