using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM
        {
            private class IGMData_Party : IGMData
            {
                private int vSpace;
                private Dictionary<Enum, FF8String> strings;

                public IGMData_Party() : base( 3, 12)
                {
                }

                public override void ReInit()
                {
                    base.ReInit();
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    {
                        CONTAINER = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 234, X = 20, Y = 84 });
                        vSpace = 0;
                    }
                    else
                    {
                        CONTAINER = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 462, X = 20, Y = 84 });
                        vSpace = 6;
                    }
                    for (int i = 0; i < 3; i++)
                        SIZE[i] = new Rectangle { Width = Width, Height = (Height / 3) - vSpace, X = X, Y = Y + (Height / 3 * i) };
                }

                public override bool Update()
                {
                    bool ret = base.Update();
                    for (sbyte i = 0; Memory.State.PartyData != null && i < SIZE.Length; i++)
                        Update(i, Memory.State.PartyData[i], Memory.State.Party[i]);
                    return true;
                }

                protected override void Init()
                {
                    strings = new Dictionary<Enum, FF8String>()
                    {
                        { Items.CurrentEXP, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,23)  },
                        { Items.NextLEVEL, Memory.Strings.Read(Strings.FileID.MNGRP, 0 ,24)  },
                    };
                    base.Init();
                }

                private void Update(sbyte pos, Characters character, Characters visableCharacter)
                {
                    if (SIZE != null)
                    {
                        if (character != Characters.Blank)
                        {
                            float yoff = 6;

                            ITEM[pos, 0] = new IGMDataItem_Box(Memory.Strings.GetName(visableCharacter), title: Icons.ID.STATUS);
                            Tuple<Rectangle, Point, Rectangle> dims = DrawBox(SIZE[pos], ((IGMDataItem_Box)ITEM[pos, 0]).Data, options: Box_Options.SkipDraw);
                            Rectangle r = dims.Item3;
                            ITEM[pos, 0].Pos = dims.Item1;
                            CURSOR[pos] = dims.Item2;

                            r = dims.Item3;
                            r.Offset(184, yoff);
                            ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r, 13);

                            r = dims.Item3;
                            r.Offset((229), yoff);
                            ITEM[pos, 2] = new IGMDataItem_Int(Memory.State.Characters[character].Level, r, 2, 0, 1,3);

                            r = dims.Item3;
                            r.Offset(304, yoff);
                            ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r, 13);

                            r = dims.Item3;
                            r.Offset((354), yoff);
                            ITEM[pos, 4] = new IGMDataItem_Int(Memory.State.Characters[character].CurrentHP(visableCharacter), r, 2, 0, 1,4);

                            r = dims.Item3;
                            r.Offset(437, yoff);
                            ITEM[pos, 5] = new IGMDataItem_Icon(Icons.ID.Slash_Forward, r, 13);

                            r = dims.Item3;

                            r.Offset((459), yoff);
                            ITEM[pos, 6] = new IGMDataItem_Int(Memory.State.Characters[character].MaxHP(visableCharacter), r, 2, 0,1,4);

                            if (Memory.State.TeamLaguna || Memory.State.SmallTeam)
                            {
                                BLANKS[pos] = false;
                                r = dims.Item3;
                                r.Offset(145, 36);
                                ITEM[pos, 7] = new IGMDataItem_String(strings[Items.CurrentEXP] + new FF8String("\n") + strings[Items.NextLEVEL], r);

                                r = dims.Item3;
                                r.Offset((340), 42);
                                ITEM[pos, 8] = new IGMDataItem_Int((int)Memory.State.Characters[character].Experience, r, 2, 0, 1,9);

                                r = dims.Item3;
                                r.Offset(520, 42);
                                ITEM[pos, 9] = new IGMDataItem_Icon(Icons.ID.P, r, 2);

                                r = dims.Item3;
                                r.Offset((340), 75);
                                ITEM[pos, 10] = new IGMDataItem_Int(Memory.State.Characters[character].ExperienceToNextLevel, r, 2, 0, 1,9);

                                r = dims.Item3;
                                r.Offset(520, 75);
                                ITEM[pos, 11] = new IGMDataItem_Icon(Icons.ID.P, r, 2);
                            }
                            else
                                for (int i = 7; i < Depth; i++)
                                    ITEM[pos, i] = null;
                        }
                        else
                        {
                            ITEM[pos, 0] = new IGMDataItem_Box(pos: SIZE[pos]);
                            BLANKS[pos] = true;
                            for (int i = 1; i < Depth; i++)
                            {
                                ITEM[pos, i] = null;
                            }
                        }
                    }
                }
            }

        }
    }
}