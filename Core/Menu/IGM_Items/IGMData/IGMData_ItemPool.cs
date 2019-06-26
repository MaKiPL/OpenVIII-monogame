using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {

        #region Classes

        private partial class IGM_Items
        {

            #region Classes

            private class IGMData_ItemPool : IGMData_Pool<Saves.Data, Item_In_Menu>
            {

                #region Fields

                private FF8String[] _helpStr;

                private bool eventSet = false;

                #endregion Fields

                #region Constructors

                public IGMData_ItemPool() : base(11, 2, new IGMDataItem_Box(pos: new Rectangle(5, 150, 415, 480), title: Icons.ID.ITEM), 11, 18)
                {
                }

                #endregion Constructors

                #region Properties

                public IReadOnlyList<FF8String> HelpStr => _helpStr;

                #endregion Properties

                #region Methods

                public override void Draw() => base.Draw();

                public override bool Inputs()
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    return base.Inputs();
                }

                public override void Inputs_CANCEL()
                {
                    InGameMenu_Items.SetMode(Mode.TopMenu);
                    base.Inputs_CANCEL();
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ReInitCompletedHandler += ReInitCompletedEvent;
                        eventSet = true;
                    }
                    base.ReInit();
                    Source = Memory.State;
                    if (Source != null && Source.Items != null)
                    {
                        ((IGMDataItem_Box)CONTAINER).Title = Pages <= 1 ? (Icons.ID?)Icons.ID.ITEM : (Icons.ID?)(Icons.ID.ITEM_PG1 + (byte)Page);
                        byte pos = 0;
                        int skip = Page * rows;
                        for (byte i = 0; pos < rows && i < Source.Items.Length; i++)
                        {
                            Saves.Item item = Source.Items[i];
                            if (item.ID == 0) continue; // skip empty values.
                            if (skip-- > 0) continue; //skip items that are on prev pages.
                            Item_In_Menu itemdata = item.DATA?? new Item_In_Menu();
                            if (itemdata.ID == 0) continue; // skip empty values.
                            ((IGMDataItem_String)(ITEM[pos, 0])).Data = itemdata.Name;
                            ((IGMDataItem_String)(ITEM[pos, 0])).Icon = itemdata.Icon;
                            ((IGMDataItem_String)(ITEM[pos, 0])).Palette = itemdata.Palette;
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Data = item.QTY;
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Show();
                            _helpStr[pos] = itemdata.Description;
                            Contents[pos] = itemdata;
                            BLANKS[pos] = false;
                            pos++;
                        }
                        for (; pos < rows; pos++)
                        {
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Hide();
                            if (pos == 0) return; // if page turning. this till be enough to trigger a try next page.
                            ((IGMDataItem_String)(ITEM[pos, 0])).Data = null;
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Data = 0;
                            ((IGMDataItem_String)(ITEM[pos, 0])).Icon = Icons.ID.None;
                            BLANKS[pos] = true;
                        }
                    }
                }

                private void ReInitCompletedEvent(object sender, EventArgs e)
                {
                    InGameMenu_Items.ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));
                }

                protected override void Init()
                {
                    base.Init();
                    _helpStr = new FF8String[Count];
                    for (byte pos = 0; pos < rows; pos++)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(null, SIZE[pos]);
                        ITEM[pos, 1] = new IGMDataItem_Int(0, new Rectangle(SIZE[pos].X + SIZE[pos].Width - 60, SIZE[pos].Y, 0, 0), numtype: Icons.NumType.sysFntBig, spaces: 3);
                    }
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-18, -20);
                    SIZE[i].Y -= 5 * row;
                    SIZE[i].Height = (int)(12 * TextScale.Y);
                }

                protected override void PAGE_NEXT()
                {
                    int cnt = Pages;
                    do
                    {
                        base.PAGE_NEXT();
                        ReInit();
                        skipsnd = true;
                    }
                    while (cnt-- > 0 && !((IGMDataItem_Int)(ITEM[0, 1])).Enabled);
                    InGameMenu_Items.ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));
                }

                protected override void PAGE_PREV()
                {
                    int cnt = Pages;
                    do
                    {
                        base.PAGE_PREV();
                        ReInit();

                        skipsnd = true;
                    }
                    while (cnt-- > 0 && !((IGMDataItem_Int)(ITEM[0, 1])).Enabled);
                    InGameMenu_Items.ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[CURSOR_SELECT], HelpStr[CURSOR_SELECT]));
                }
                protected override void SetCursor_select(int value)
                {
                    if (value != GetCursor_select())
                    {
                        base.SetCursor_select(value);
                        InGameMenu_Items.ItemChangeHandler?.Invoke(this, new KeyValuePair<Item_In_Menu, FF8String>(Contents[value], HelpStr[value]));
                    }
                }
                public override void Inputs_OKAY()
                {
                    var item = Contents[CURSOR_SELECT];
                    if (item.Target == Item_In_Menu._Target.None)
                        return;
                    base.Inputs_OKAY();
                    InGameMenu_Items.SetMode(Mode.UseItemOnTarget);
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                    if (e == Mode.SelectItem)
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                    }
                    else if (e == Mode.UseItemOnTarget)
                    {
                        Cursor_Status |= Cursor_Status.Blinking;
                    }
                    else
                    {
                        Cursor_Status &= ~Cursor_Status.Enabled;
                    }
                }

                #endregion Methods

            }

            #endregion Classes

        }

        #endregion Classes

    }
}