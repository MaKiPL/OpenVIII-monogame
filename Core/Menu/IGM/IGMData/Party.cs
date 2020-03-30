using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class Party : IGMData.Base
        {
            #region Fields

            private bool _skipRefresh;
            private int _vSpace;

            #endregion Fields

            #region Properties

            public Damageable[] Contents { get; private set; }

            #endregion Properties

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Party Create() => Create<Party>(3, 12, new IGMDataItem.Empty(), 1, 3);

            public override void Refresh()
            {
                if (Memory.State?.Characters == null || _skipRefresh) return;
                //skipReInit = true;
                //IGMDataItem.Empty c;
                if (!Memory.State.TeamLaguna && !Memory.State.SmallTeam)
                {
                    CONTAINER.Pos = new Rectangle { Width = 580, Height = 234, X = 20, Y = 84 };
                    _vSpace = 0;
                }
                else
                {
                    CONTAINER.Pos = new Rectangle { Width = 580, Height = 462, X = 20, Y = 84 };
                    _vSpace = 6;
                }
                InitSize(true);

                if (Memory.State.Characters != null)
                {
                    base.Update();
                    for (sbyte i = 0; Memory.State.PartyData != null && i < SIZE.Length; i++)
                    {
                        var cid = Memory.State.PartyData[i];
                        if (cid != Characters.Blank)
                            RefreshCharacter(i, Memory.State[cid]);
                        else
                            BlankArea(i);
                    }
                }
                _skipRefresh = false;
            }

            protected override void Init()
            {
                Contents = new Damageable[Count];
                base.Init();
                for (var pos = 0; pos < Count; pos++)
                {
                    ITEM[pos, 0] = new IGMDataItem.Box { Title = Icons.ID.STATUS };
                    ITEM[pos, 1] = new IGMDataItem.Icon { Data = Icons.ID.Lv, Palette = 13 };
                    ITEM[pos, 2] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 3 };
                    ITEM[pos, 3] = new IGMDataItem.Icon { Data = Icons.ID.HP2, Palette = 13 };
                    ITEM[pos, 4] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 4 };
                    ITEM[pos, 5] = new IGMDataItem.Icon { Data = Icons.ID.Slash_Forward, Palette = 13 };
                    ITEM[pos, 6] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 4 };
                    ITEM[pos, 7] = new IGMDataItem.Text();
                    ITEM[pos, 8] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 9 };
                    ITEM[pos, 9] = new IGMDataItem.Icon { Data = Icons.ID.P, Palette = 2 };
                    ITEM[pos, 10] = new IGMDataItem.Integer { Palette = 2, Faded_Palette = 0, Padding = 1, Spaces = 9 };
                    ITEM[pos, 11] = new IGMDataItem.Icon { Data = Icons.ID.P, Palette = 2 };
                    for (var i = 0; i < Depth; i++)
                        ITEM[pos, i].Hide();
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Height -= _vSpace;
            }

            private void BlankArea(sbyte pos)
            {
                ((IGMDataItem.Box)ITEM[pos, 0]).Data = "";
                ((IGMDataItem.Box)ITEM[pos, 0]).Title = Icons.ID.None;
                ((IGMDataItem.Box)ITEM[pos, 0]).Pos = SIZE[pos];
                ((IGMDataItem.Box)ITEM[pos, 0]).Show();
                BLANKS[pos] = true;
                for (var i = 1; i < Depth; i++)
                {
                    ITEM[pos, i].Hide();
                }
            }

            private void RefreshCharacter(sbyte pos, Damageable damageable)
            {
                if (SIZE == null) return;
                ((IGMDataItem.Box)ITEM[pos, 0]).Pos = SIZE[pos];
                if (damageable != null)
                {
                    Contents[pos] = damageable;
                    const float offsetY = 6;

                    ((IGMDataItem.Box)ITEM[pos, 0]).Data = damageable.Name;
                    ((IGMDataItem.Box)ITEM[pos, 0]).Title = Icons.ID.STATUS;
                    var dims = DrawBox(SIZE[pos], ((IGMDataItem.Box)ITEM[pos, 0]).Data, options: Box_Options.SkipDraw);
                    CURSOR[pos] = dims.Cursor;

                    var r = dims.Font;
                    r.Offset(184, offsetY);
                    ((IGMDataItem.Icon)ITEM[pos, 1]).Pos = r;

                    r = dims.Font;
                    r.Offset((229), offsetY);
                    ((IGMDataItem.Integer)ITEM[pos, 2]).Data = damageable.Level;
                    ((IGMDataItem.Integer)ITEM[pos, 2]).Pos = r;

                    r = dims.Font;
                    r.Offset(304, offsetY);
                    ((IGMDataItem.Icon)ITEM[pos, 3]).Pos = r;

                    r = dims.Font;
                    r.Offset((354), offsetY);
                    ((IGMDataItem.Integer)ITEM[pos, 4]).Data = damageable.CurrentHP();
                    ((IGMDataItem.Integer)ITEM[pos, 4]).Pos = r;

                    r = dims.Font;
                    r.Offset(437, offsetY);
                    ((IGMDataItem.Icon)ITEM[pos, 5]).Pos = r;

                    r = dims.Font;
                    r.Offset((459), offsetY);
                    ((IGMDataItem.Integer)ITEM[pos, 6]).Data = damageable.MaxHP();
                    ((IGMDataItem.Integer)ITEM[pos, 6]).Pos = r;

                    for (var i = 0; i <= 6; i++)
                        ITEM[pos, i].Show();
                    if ((Memory.State.TeamLaguna || Memory.State.SmallTeam) && damageable.GetCharacterData(out var c))
                    {
                        BLANKS[pos] = false;
                        r = dims.Font;
                        r.Offset(145, 36);
                        var s = Strings.Name.CurrentEXP + "\n" + Strings.Name.NextLEVEL;
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

                        for (var i = 7; i < Depth; i++)
                            ITEM[pos, i].Show();
                    }
                    else
                        for (var i = 7; i < Depth; i++)
                            ITEM[pos, i].Hide();
                }
                else
                {
                    BlankArea(pos);
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}