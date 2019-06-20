using Microsoft.Xna.Framework;
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

            protected override void Init()
            {
                Size = new Vector2 { X = 840, Y = 630 };
                TextScale = new Vector2(2.545455f, 3.0375f);

                Data.Add(SectionName.Help, new IGMData_Container(
                    new IGMDataItem_Box(null, pos: new Rectangle(15, 69, 810, 78), Icons.ID.HELP, options: Box_Options.Middle)));
                Data.Add(SectionName.TopMenu, new IGMData_TopMenu());
                Data.Add(SectionName.Title, new IGMData_Container(
                    new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2), pos: new Rectangle(615, 0, 225, 66))));
                Data.Add(SectionName.UseItemGroup, new IGMData_Group(
                    new IGMData_ItemPool(),
                    new IGMData_CharacterPool(),
                    new IGMData_Statuses()
                    ));
                base.Init();
            }


            protected override bool Inputs() => false;

            private class IGMData_TopMenu : IGMData
            {
                private FF8String[] _helpStr;

                public IGMData_TopMenu() : base(4, 1, new IGMDataItem_Box(pos: new Rectangle(0, 12, 610, 54)), 4, 1)
                {
                }

                public IReadOnlyList<FF8String> HelpStr => _helpStr;

                protected override void Init()
                {
                    base.Init();
                    _helpStr = new FF8String[Count];
                    for (byte pos = 0; pos < Count; pos++)
                    {
                        FF8String data = null;
                        switch (pos)
                        {
                            case 0:
                                data = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 179);//Use
                                _helpStr[pos] = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 180);
                                break;

                            case 1:
                                data = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 183);//Rearrange
                                _helpStr[pos] = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 184);
                                break;

                            case 2:
                                data = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 202);//Sort
                                _helpStr[pos] = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 203);
                                break;

                            case 3:
                                data = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 181);//Battle
                                _helpStr[pos] = Memory.Strings.Read(Strings.FileID.MNGRP, 2, 182);
                                break;
                        }
                        ITEM[pos, 0] = new IGMDataItem_String(data, SIZE[pos]);
                    }
                    Cursor_Status |= Cursor_Status.Enabled;
                    Cursor_Status |= Cursor_Status.Horizontal;
                    Cursor_Status |= Cursor_Status.Vertical;
                    Cursor_Status |= Cursor_Status.Blinking;
                }
            }

            private class IGMData_ItemPool : IGMData_Pool<Saves.Data, byte>
            {
                public IGMData_ItemPool() : base(11, 2, new IGMDataItem_Box(pos: new Rectangle(5, 150, 415, 480), title: Icons.ID.ITEM), 11, 1)
                {
                }

                private FF8String[] _helpStr;
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

                public override void ReInit()
                {
                    base.ReInit();
                    Source = Memory.State;
                    if (Source != null && Source.Items != null)
                    {
                        byte pos = 0;
                        for (byte i = 0; pos < rows && i < Source.Items.Length; i++)
                        {
                            Saves.Item item = Source.Items[i];
                            //byte itembo = Source.Itemsbattleorder[i];
                            if (item.ID == 0) continue;
                            Kernel_bin.Battle_Items_Data bitemdata = Kernel_bin.BattleItemsData.Count > item.ID ? Kernel_bin.BattleItemsData[item.ID] : null;
                            Kernel_bin.Non_battle_Items_Data nbitemdata = bitemdata == null ? Kernel_bin.NonbattleItemsData[item.ID - Kernel_bin.BattleItemsData.Count] : null;
                            Item_In_Menu itemdata = Memory.MItems.Items[item.ID];

                            _helpStr[pos] = bitemdata == null ? nbitemdata.Description : bitemdata.Description;
                            ((IGMDataItem_String)(ITEM[pos, 0])).Data = bitemdata == null ? nbitemdata.Name : bitemdata.Name;
                            ((IGMDataItem_Int)(ITEM[pos, 1])).Data = item.QTY;
                            pos++;
                        }
                    }
                }
            }

            private class IGMData_CharacterPool : IGMData_Pool<Saves.Data, Characters>
            {
                public IGMData_CharacterPool() : base(9, 3, new IGMDataItem_Box(pos: new Rectangle(420, 150, 420, 360), title: Icons.ID.NAME), 9, 1)
                {
                }
            }

            private class IGMData_Statuses : IGMData
            {
                public IGMData_Statuses() : base(1, 1, new IGMDataItem_Box(pos: new Rectangle(420, 510, 420, 120)))
                {
                }
            }
        }
    }
}