using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_Party : IGMData
        {
            #region Fields

            private bool skipReInit = false;
            private Dictionary<Enum, FF8String> strings;
            private int vSpace;

            #endregion Fields

            #region Constructors

            public IGMData_Party() : base()
            {
            }

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
                    IGMDataItem_Empty c;
                    if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                    {
                        c = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 234, X = 20, Y = 84 });
                        vSpace = 0;
                    }
                    else
                    {
                        c = new IGMDataItem_Empty(pos: new Rectangle { Width = 580, Height = 462, X = 20, Y = 84 });
                        vSpace = 6;
                    }
                    Init(3, 12, c, 1, 3);

                    if (Memory.State.Characters != null)
                    {
                        bool ret = base.Update();
                        for (sbyte i = 0; Memory.State.PartyData != null && i < SIZE.Length; i++)
                            ReInitCharacter(i, Memory.State[Memory.State.PartyData[i]]);
                    }
                    skipReInit = false;
                }
            }

            protected override void Init()
            {
                Contents = new Damageable[Count];
                base.Init();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Height -= vSpace;
            }

            private void ReInitCharacter(sbyte pos, Damageable damageable)
            {
                if (SIZE != null)
                {
                    if (damageable != null)
                    {
                        Contents[pos] = damageable;
                        float yoff = 6;

                        ITEM[pos, 0] = new IGMDataItem_Box(damageable.Name, title: Icons.ID.STATUS);
                        Tuple<Rectangle, Point, Rectangle> dims = DrawBox(SIZE[pos], ((IGMDataItem_Box)ITEM[pos, 0]).Data, options: Box_Options.SkipDraw);
                        Rectangle r = dims.Item3;
                        ((IGMDataItem_Box)ITEM[pos, 0]).Pos = dims.Item1;
                        CURSOR[pos] = dims.Item2;

                        r = dims.Item3;
                        r.Offset(184, yoff);
                        ITEM[pos, 1] = new IGMDataItem_Icon(Icons.ID.Lv, r, 13);

                        r = dims.Item3;
                        r.Offset((229), yoff);
                        ITEM[pos, 2] = new IGMDataItem_Int(damageable.Level, r, 2, 0, 1, 3);

                        r = dims.Item3;
                        r.Offset(304, yoff);
                        ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.HP2, r, 13);

                        r = dims.Item3;
                        r.Offset((354), yoff);
                        ITEM[pos, 4] = new IGMDataItem_Int(damageable.CurrentHP(), r, 2, 0, 1, 4);

                        r = dims.Item3;
                        r.Offset(437, yoff);
                        ITEM[pos, 5] = new IGMDataItem_Icon(Icons.ID.Slash_Forward, r, 13);

                        r = dims.Item3;

                        r.Offset((459), yoff);
                        ITEM[pos, 6] = new IGMDataItem_Int(damageable.MaxHP(), r, 2, 0, 1, 4);

                        if ((Memory.State.TeamLaguna || Memory.State.SmallTeam) && Damageable.GetCharacterData(out Saves.CharacterData c))
                        {
                            BLANKS[pos] = false;
                            r = dims.Item3;
                            r.Offset(145, 36);
                            FF8String s = Strings[Items.CurrentEXP] + "\n" + Strings[Items.NextLEVEL];
                            ITEM[pos, 7] = new IGMDataItem_String(s, r);

                            r = dims.Item3;
                            r.Offset((340), 42);
                            ITEM[pos, 8] = new IGMDataItem_Int(checked((int)c.Experience), r, 2, 0, 1, 9);

                            r = dims.Item3;
                            r.Offset(520, 42);
                            ITEM[pos, 9] = new IGMDataItem_Icon(Icons.ID.P, r, 2);

                            r = dims.Item3;
                            r.Offset((340), 75);
                            ITEM[pos, 10] = new IGMDataItem_Int(c.ExperienceToNextLevel, r, 2, 0, 1, 9);

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

            #endregion Methods
        }

        #endregion Classes
    }
}