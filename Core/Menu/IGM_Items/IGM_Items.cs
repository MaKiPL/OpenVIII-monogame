using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {
        private class IGM_Items : Menu
        {
            public enum SectionName : byte
            {
                TopMenu,
                UseItemGroup,
                Help,
                Title,
            }

            public enum Mode : byte
            {
                /// <summary>
                /// Select one of the 4 top options to do
                /// </summary>
                TopMenu,

                /// <summary>
                /// Show Item pool and characters by default, choose an item to use
                /// </summary>
                UseItemPool,

                /// <summary>
                /// If item is a gf item type show gfs instead of characters, choose an item to use
                /// </summary>
                UseItemPoolGFItem,

                /// <summary>
                /// Choose a character to use item on
                /// </summary>
                UseItemChooseCharacter,

                /// <summary>
                /// Choose a GF to use item on.
                /// </summary>
                UseItemChooseGF,
            }

            protected override void Init()
            {
                Size = new Vector2 { X = 840, Y = 630 };
                TextScale = new Vector2(2.545455f, 3.0375f);

                Data.Add(SectionName.Help, new IGMData_Container(
                    new IGMDataItem_Box(null, pos: new Rectangle(15, 69, 810, 78), Icons.ID.HELP, options: Box_Options.Middle)));
                Data.Add(SectionName.TopMenu, new IGMData_TopMenu(new Dictionary<FF8String, FF8String>() {
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 179),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 180)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 183),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 184)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 202),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 203)},
                            { Memory.Strings.Read(Strings.FileID.MNGRP, 2, 181),Memory.Strings.Read(Strings.FileID.MNGRP, 2, 182)},
                            }));
                Data.Add(SectionName.Title, new IGMData_Container(
                    new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), pos: new Rectangle(615, 0, 225, 66))));
                Data.Add(SectionName.UseItemGroup, new IGMData_Group(
                    new IGMData_ItemPool(),
                    new IGMData_CharacterPool(),
                    new IGMData_Statuses()
                    ));
                InputsDict = new Dictionary<Mode, Func<bool>>() {
                {Mode.TopMenu, Data[SectionName.TopMenu].Inputs},
                {Mode.UseItemPool, ((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[0,0]).Inputs},
                {Mode.UseItemPoolGFItem, ((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[0,0]).Inputs},
                {Mode.UseItemChooseCharacter, ((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[0,0]).Inputs},
                {Mode.UseItemChooseGF,((IGMDataItem_IGMData)((IGMData_Group)Data[SectionName.UseItemGroup]).ITEM[0,0]).Inputs},
                };
                SetMode(Mode.UseItemPool);
                base.Init();
            }
            public EventHandler<Mode> ModeChangeHandler;
            private Mode mode;

            public Mode GetMode() => mode;

            public void SetMode(Mode value)
            {
                if (mode != value)
                {
                    ModeChangeHandler?.Invoke(this, value);
                    mode = value;
                }
            }

            protected Dictionary<Mode, Func<bool>> InputsDict;

            protected override bool Inputs() => InputsDict[GetMode()]();

            private class IGMData_TopMenu : IGMData
            {
                private FF8String[] _helpStr;
                private int[] widths;
                bool eventSet = false;

                private IReadOnlyDictionary<FF8String, FF8String> Pairs { get; }
                public IGMData_TopMenu(IReadOnlyDictionary<FF8String, FF8String> pairs) : base()
                {
                    Pairs = pairs;
                    _helpStr = new FF8String[Pairs.Count];
                    widths = new int[Pairs.Count];
                    byte pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in Pairs)
                    {
                        _helpStr[pos] = pair.Value;
                        Rectangle rectangle = Memory.font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                        widths[pos] = rectangle.Width;
                        if (rectangle.Width > largestwidth) largestwidth = rectangle.Width;
                        if (rectangle.Height > largestheight) largestheight = rectangle.Height;
                        totalwidth += rectangle.Width;


                        avgwidth = totalwidth / ++pos;
                    }
                    Init(Pairs.Count, 1, new IGMDataItem_Box(pos: new Rectangle(0, 12, 610, 54)), Pairs.Count, 1);
                    pos = 0;
                    foreach (KeyValuePair<FF8String, FF8String> pair in Pairs)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(pair.Key, SIZE[pos]);
                        pos++;
                    }
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status |= Cursor_Status.Blinking;
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();
                }
                private void ModeChangeEvent(object sender, Mode e)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    SIZE[i].Inflate(0, (SIZE[i].Height - largestheight)/-2);
                    SIZE[i].X += SIZE[i].Width / 2 - widths[i]/2;
                    SIZE[i].Width = widths[i];
                    SIZE[i].Height = largestheight;
                }

                public IReadOnlyList<FF8String> HelpStr => _helpStr;

                protected int largestwidth { get; private set; }
                protected int totalwidth { get; private set; }
                protected int avgwidth { get; private set; }
                protected int largestheight { get; private set; }
            }

            private class IGMData_ItemPool : IGMData_Pool<Saves.Data, byte>
            {
                public IGMData_ItemPool() : base(11, 2, new IGMDataItem_Box(pos: new Rectangle(5, 150, 415, 480), title: Icons.ID.ITEM), 11, 1)
                {
                }

                private FF8String[] _helpStr;
                private bool eventSet=false;

                public IReadOnlyList<FF8String> HelpStr => _helpStr;

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
                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                    base.ReInit();
                    Source = Memory.State;
                    if (Source != null && Source.Items != null)
                    {
                        Cursor_Status |= Cursor_Status.Enabled;
                        Cursor_Status |= Cursor_Status.Vertical;
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        byte pos = 0;
                        for (byte i = 0; pos < rows && i < Source.Items.Length; i++)
                        {
                            Saves.Item item = Source.Items[i];
                            //byte itembo = Source.Itemsbattleorder[i];
                            if (item.ID == 0) continue;
                            Kernel_bin.Battle_Items_Data bitemdata = Kernel_bin.BattleItemsData.Count > item.ID ? Kernel_bin.BattleItemsData[item.ID] : null;
                            Kernel_bin.Non_battle_Items_Data nbitemdata = bitemdata == null ? Kernel_bin.NonbattleItemsData[item.ID - Kernel_bin.BattleItemsData.Count] : null;
                            Item_In_Menu itemdata = Memory.MItems.Items[item.ID];
                            ((IGMDataItem_String)(ITEM[pos, 0])).Data = bitemdata == null ? nbitemdata.Name : bitemdata.Name;
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Data = item.QTY;
                            _helpStr[pos] = bitemdata == null ? nbitemdata.Description : bitemdata.Description;
                            BLANKS[pos] = false;
                            pos++;
                        }
                    }                    
                }
                public override bool Inputs()
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    return base.Inputs();
                }
                public override void Draw()
                {
                    base.Draw();
                }
            }

            private class IGMData_CharacterPool : IGMData_Pool<Saves.Data, Characters>
            {
                private bool eventSet;

                public IGMData_CharacterPool() : base(9, 3, new IGMDataItem_Box(pos: new Rectangle(420, 150, 420, 360), title: Icons.ID.NAME), 9, 1)
                {
                }
                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                }
                public override void Draw()
                {
                    //base.Draw();
                }
            }

            private class IGMData_Statuses : IGMData
            {
                private bool eventSet;

                public IGMData_Statuses() : base(1, 1, new IGMDataItem_Box(pos: new Rectangle(420, 510, 420, 120)))
                {
                }
                private void ModeChangeEvent(object sender, Mode e)
                {
                }
                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        eventSet = true;
                    }
                }
                public override void Draw()
                {
                    base.Draw();
                }
            }

        }
    }
}