using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Party : IGMData.Base
        {
            #region Fields

            private bool skipReInit = false;
            private Dictionary<Enum, FF8String> strings;
            private int vSpace;

            #endregion Fields

            #region Constructors

            public static IGMData_Party Create() => Create<IGMData_Party>();

            #endregion Constructors

            #region Properties

            public Damageable[] Contents { get; private set; }
            protected IReadOnlyDictionary<Enum, FF8String> Strings
            {
                get
                {
                    if (strings == null)
                        strings = new Dictionary<Enum, FF8String>()
                            {
                                { Items.CurrentEXP, Memory.Strings.Read(OpenVIII.Strings.FileID.MNGRP, 0 ,23)  },
                                { Items.NextLEVEL, Memory.Strings.Read(OpenVIII.Strings.FileID.MNGRP, 0 ,24)  },
                            };
                    return strings;
                }
            }

            #endregion Properties

            #region Methods

            public override void Refresh()
            {
                if (Memory.State.Characters != null && !skipReInit)
                {
                    skipReInit = true;
                    IGMDataItem.Empty c;
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    {
                        c = new IGMDataItem.Empty(pos: new Rectangle { Width = 580, Height = 234, X = 20, Y = 84 });
                        vSpace = 0;
                    }
                    else
                    {
                        c = new IGMDataItem.Empty(pos: new Rectangle { Width = 580, Height = 462, X = 20, Y = 84 });
                        vSpace = 6;
                    }
                    Init(3, 12, c, 1, 3);

                    if (Memory.State.Characters != null)
                    {
                        bool ret = base.Update();
                        for (sbyte i = 0; Memory.State.PartyData != null && i < SIZE.Length; i++)
                        {
                            Characters cid = Memory.State.PartyData[i];
                            if (cid != Characters.Blank)
                                RefreshCharacter(i, Memory.State[cid]);
                            else
                                BlankArea(i);
                                
                        }
                    }
                    skipReInit = false;
                }
            }

            protected override void Init()
            {
                Contents = new Damageable[Count];
                base.Init();
                for (int pos = 0; pos < Count; pos++)
                {
                    ITEM[pos, 0] = new IGMDataItem.Box { Title = Icons.ID.STATUS };
                    ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.Lv, Palette = 13 };
                    ITEM[pos, 2] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 3 };
                    ITEM[pos, 3] = new IGMDataItem.Icon { Data = Icons.ID.HP2, Palette = 13 };
                    ITEM[pos, 4] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 4 };
                    ITEM[pos, 5] = new IGMDataItem.Icon { Data = Icons.ID.Slash_Forward, Palette = 13 };
                    ITEM[pos, 6] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 4 };
                    ITEM[pos, 7] = new IGMDataItem.Text { };
                    ITEM[pos, 8] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 9 };
                    ITEM[pos, 9] = new IGMDataItem.Icon { Data = Icons.ID.P, Palette = 2 };
                    ITEM[pos, 10] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 9 };
                    ITEM[pos, 11] = new IGMDataItem.Icon { Data = Icons.ID.P, Palette = 2 };
                    for (int i = 0; i < Depth; i++)
                        ITEM[pos, i].Hide();
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Height -= vSpace;
            }

            private void RefreshCharacter(sbyte pos, Damageable damageable)
            {
                if (SIZE != null)
                {
                    ((IGMDataItem.Box)ITEM[pos, 0]).Pos = SIZE[pos];
                    if (damageable != null)
                    {
                        Contents[pos] = damageable;
                        float yoff = 6;

                        ((IGMDataItem.Box)ITEM[pos, 0]).Data = damageable.Name;
                        ((IGMDataItem.Box)ITEM[pos, 0]).Title = Icons.ID.STATUS;
                        BoxReturn dims = DrawBox(SIZE[pos], ((IGMDataItem.Box)ITEM[pos, 0]).Data, options: Box_Options.SkipDraw);
                        Rectangle r = dims.Font;
                        CURSOR[pos] = dims.Cursor;

                        r = dims.Font;
                        r.Offset(184, yoff);
                        ((IGMDataItem.Icon)ITEM[pos, 1]).Pos = r;

                        r = dims.Font;
                        r.Offset((229), yoff);
                        ((IGMDataItem.Integer)ITEM[pos, 2]).Data = damageable.Level;
                        ((IGMDataItem.Integer)ITEM[pos, 2]).Pos = r;

                        r = dims.Font;
                        r.Offset(304, yoff);
                        ((IGMDataItem.Icon)ITEM[pos, 3]).Pos = r;

                        r = dims.Font;
                        r.Offset((354), yoff);
                        ((IGMDataItem.Integer)ITEM[pos, 4]).Data = damageable.CurrentHP();
                        ((IGMDataItem.Integer)ITEM[pos, 4]).Pos = r;

                        r = dims.Font;
                        r.Offset(437, yoff);
                        ((IGMDataItem.Icon)ITEM[pos, 5]).Pos = r;

                        r = dims.Font;
                        r.Offset((459), yoff);
                        ((IGMDataItem.Integer)ITEM[pos, 6]).Data = damageable.MaxHP();
                        ((IGMDataItem.Integer)ITEM[pos, 6]).Pos = r;

                        for (int i = 0; i <= 6; i++)
                            ITEM[pos, i].Show();
                        if ((Memory.State.TeamLaguna || Memory.State.SmallTeam) && Damageable.GetCharacterData(out Saves.CharacterData c))
                        {
                            BLANKS[pos] = false;
                            r = dims.Font;
                            r.Offset(145, 36);
                            FF8String s = Strings[Items.CurrentEXP] + "\n" + Strings[Items.NextLEVEL];
                            ((IGMDataItem.Text)ITEM[pos, 7]).Data = s;
                            ((IGMDataItem.Text)ITEM[pos, 7]).Pos = r;

                            r = dims.Font;
                            r.Offset((340), 42);
                            ((IGMDataItem.Integer)ITEM[pos, 8]).Data = checked((int)c.Experience);
                            ((IGMDataItem.Integer)ITEM[pos, 8]).Pos = r;

                            r = dims.Font;
                            r.Offset(520, 42);
                            ((IGMDataItem.Icon)ITEM[pos, 9]).Pos = r;

                            r = dims.Font;
                            r.Offset((340), 75);
                            ((IGMDataItem.Integer)ITEM[pos, 10]).Data = c.ExperienceToNextLevel;
                            ((IGMDataItem.Integer)ITEM[pos, 10]).Pos = r;

                            r = dims.Font;
                            r.Offset(520, 75);
                            ((IGMDataItem.Icon)ITEM[pos, 11]).Pos = r;

                            for (int i = 7; i < Depth; i++)
                                ITEM[pos, i].Show();
                        }
                        else
                            for (int i = 7; i < Depth; i++)
                                ITEM[pos, i].Hide();
                    }
                    else
                    {
                        BlankArea(pos);
                    }
                }
            }

            private void BlankArea(sbyte pos)
            {
                ((IGMDataItem.Box)ITEM[pos, 0]).Data = "";
                ((IGMDataItem.Box)ITEM[pos, 0]).Title = Icons.ID.None;
                ((IGMDataItem.Box)ITEM[pos, 0]).Pos = SIZE[pos];
                ((IGMDataItem.Box)ITEM[pos, 0]).Show();
                BLANKS[pos] = true;
                for (int i = 1; i < Depth; i++)
                {
                    ITEM[pos, i].Hide();
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}