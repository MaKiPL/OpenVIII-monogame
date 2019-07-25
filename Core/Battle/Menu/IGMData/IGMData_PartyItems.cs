using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public partial class VictoryMenu
        {
            #region Classes

            private class IGMData_PartyItems : IGMData
            {
                #region Fields

                private readonly FF8String DialogSelectedItem;
                private readonly FF8String str_NotFound;
                private readonly FF8String str_Over100;
                private readonly FF8String str_Recieved;
                private Queue<Saves.Item> _items;

                #endregion Fields

                #region Constructors

                public IGMData_PartyItems(IGMDataItem container) : base()
                {
                    str_NotFound = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 28);
                    str_Over100 = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 24);
                    str_Recieved = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 6);
                    DialogSelectedItem = new byte[] { (byte)FF8TextTagCode.Dialog, (byte)FF8TextTagDialog.SelectedItem };

                    Init(1, 7, container, 1, 1);
                }

                #endregion Constructors

                #region Properties

                public Queue<Saves.Item> Items
                {
                    get => _items; set
                    {
                        _items = value;
                        Refresh();
                    }
                }

                public Saves.Item Item { get; private set; }

                #endregion Properties

                #region Methods

                public void Earn()
                {
                    skipsnd = true;
                    init_debugger_Audio.PlaySound(17);
                }

                public override bool Inputs_CANCEL() => false;

                public override void Inputs_OKAY()
                {
                    if (ITEM[0, 5].Enabled || ITEM[0, 6].Enabled)
                    {
                        if (Items != null && Items.Count > 0)
                        {
                            Refresh();
                            base.Inputs_OKAY();
                        }
                    }
                    else if (Items != null && Items.Count > 0)
                    {
                        if (Memory.State.EarnItem(Items.Dequeue()))
                        {
                            ITEM?[0, 6]?.Show();
                            Earn();
                        }
                        else
                        {
                            ITEM?[0, 5]?.Show();
                            Earn();
                        }

                        base.Inputs_OKAY();
                    }
                }

                public override void Refresh()
                {
                    base.Refresh();
                    if (Items != null && Items.Count > 0)
                    {
                        Item = Items.Peek();
                        ((IGMDataItem_Box)ITEM[0, 1]).Data = Item.DATA?.Name;
                        ((IGMDataItem_Box)ITEM[0, 2]).Data = $"{Item.QTY}";
                        ((IGMDataItem_Box)ITEM[0, 3]).Data = Item.DATA?.Description;
                        ((IGMData_SmallMsgBox)((IGMDataItem_IGMData)ITEM[0, 5]).Data).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ((IGMData_SmallMsgBox)((IGMDataItem_IGMData)ITEM[0, 5]).Data).Data = str_Over100.Clone().Replace(DialogSelectedItem, Item.DATA?.Name);
                        ITEM[0, 1].Show();
                        ITEM[0, 2].Show();
                        ITEM[0, 3].Show();
                        ITEM[0, 4].Hide();
                        ITEM[0, 5].Hide();
                        ITEM[0, 6].Hide();
                    }
                    else
                    {
                        ITEM?[0, 1]?.Hide();
                        ITEM?[0, 2]?.Hide();
                        ITEM?[0, 3]?.Hide();
                        ITEM?[0, 4]?.Show();
                        ITEM?[0, 5]?.Hide();
                        ITEM?[0, 6]?.Hide();
                    }
                }

                protected override void Init()
                {
                    base.Init();
                    ITEM[0, 0] = new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL, 30, 21), new Rectangle(SIZE[0].X, SIZE[0].Y, SIZE[0].Width, 78), Icons.ID.INFO, options: Box_Options.Middle);
                    ITEM[0, 1] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X + 140, SIZE[0].Y + 189, 475, 78), Icons.ID.ITEM, options: Box_Options.Middle); // item name
                    ITEM[0, 2] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X + 615, SIZE[0].Y + 189, 125, 78), Icons.ID.NUM_, options: Box_Options.Middle | Box_Options.Center); // item count
                    ITEM[0, 3] = new IGMDataItem_Box(null, new Rectangle(SIZE[0].X, SIZE[0].Y + 444, SIZE[0].Width, 78), Icons.ID.HELP, options: Box_Options.Middle); // item description
                    ITEM[0, 4] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center | Box_Options.Middle, SIZE[0])); // Couldn't find any items
                    ITEM[0, 5] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(null, SIZE[0].X + 230, SIZE[0].Y + 291, Icons.ID.NOTICE, Box_Options.Center, SIZE[0])); // over 100 discarded
                    ITEM[0, 6] = new IGMDataItem_IGMData(new IGMData_SmallMsgBox(null, SIZE[0].X + 232, SIZE[0].Y + 315, Icons.ID.NOTICE, Box_Options.Center, SIZE[0])); // Recieved item
                    Cursor_Status |= (Cursor_Status.Hidden | (Cursor_Status.Enabled | Cursor_Status.Static));
                }

                #endregion Methods
            }

            #endregion Classes
        }

        #endregion Classes
    }
}