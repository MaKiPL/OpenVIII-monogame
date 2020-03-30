using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGMItems
    {
        #region Enums

        public enum Items
        {
            HP,
            LV,
            ForwardSlash
        }

        #endregion Enums

        #region Classes

        private class Statuses : IGMData.Base
        {
            #region Fields

            private bool _eventSet;
            [SuppressMessage("ReSharper", "NotAccessedField.Local")] private Dictionary<Items, FF8String> _misc;

            #endregion Fields

            #region Properties

            private bool All => (_item.ItemTarget & (ItemTarget.All | ItemTarget.All2)) != 0;
            private ItemInMenu _item;

            private Faces.ID _target = Faces.ID.Blank;

            #endregion Properties

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static Statuses Create() => Create<Statuses>(2, 4, new IGMDataItem.Box { Title = Icons.ID.STATUS, Pos = new Rectangle(420, 510, 420, 120) }, 1, 2);

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.UseItemOnTarget))
                    TargetChangeEvent(this, Faces.ID.Blank);
            }

            public override void Refresh()
            {
                if (IGMItems != null)
                {
                    if (!_eventSet)
                    {
                        IGMItems.ModeChangeHandler += ModeChangeEvent;
                        IGMItems.ChoiceChangeHandler += ChoiceChangeEvent;
                        IGMItems.ItemPool.ItemChangeHandler += ItemChangeEvent;
                        IGMItems.TargetChangeHandler += TargetChangeEvent;
                        _eventSet = true;
                    }
                    else
                        Fill(_target); // refresh the screen.
                }
            }

            protected override void Init()
            {
                _misc = new Dictionary<Items, FF8String> {
                { Items.HP,Memory.Strings.Read(Strings.FileID.MenuGroup,0,26)},
                { Items.LV,Memory.Strings.Read(Strings.FileID.MenuGroup,0,27)},
                { Items.ForwardSlash,Memory.Strings.Read(Strings.FileID.MenuGroup,0,25)},
                };
                base.Init();
                _target = Faces.ID.Blank;
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 5 * row;
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e)
            {
            }

            private void Fill(Faces.ID e)
            {
                if ((e == Faces.ID.Blank && _target != Faces.ID.Blank) || All)
                {
                    _target = e;
                    foreach (var i in ITEM)
                        i?.Hide();
                }
                else
                {
                    if (_target == Faces.ID.Blank)
                        foreach (var i in ITEM)
                            i?.Show();
                    _target = e;
                    var character = e.ToCharacters();
                    var gf = e.ToGFs();
                    if (character != Characters.Blank || (gf != GFs.Blank && gf != GFs.All))
                    {
                        ITEM[0, 0] = new IGMDataItem.Text { Data = Strings.Name.LV, Pos = new Rectangle(SIZE[0].X, SIZE[0].Y, 0, 0) };
                        ITEM[1, 0] = new IGMDataItem.Text { Data = Strings.Name.HP, Pos = new Rectangle(SIZE[1].X, SIZE[1].Y, 0, 0) };
                        ITEM[1, 2] = new IGMDataItem.Text { Data = Strings.Name.ForwardSlash, Pos = new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0) };
                    }
                    if (Memory.State.Characters && character != Characters.Blank)
                    {
                        ITEM[0, 1] = new IGMDataItem.Integer { Data = Memory.State[character].Level, Pos = new Rectangle(SIZE[0].X + 35, SIZE[0].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 6 };
                        ITEM[0, 2] = Memory.State.Party != null && Memory.State.Party.Contains(character)
                            ? new IGMDataItem.Icon { Data = Icons.ID.InParty, Pos = new Rectangle(SIZE[0].X + 155, SIZE[0].Y, 0, 0), Palette = 6 }
                            : null;
                        ITEM[1, 1] = new IGMDataItem.Integer { Data = Memory.State[character].CurrentHP(character), Pos = new Rectangle(SIZE[1].X + 35, SIZE[1].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 6 };
                        ITEM[1, 3] = new IGMDataItem.Integer { Data = Memory.State[character].MaxHP(character), Pos = new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 5 };
                    }

                    if (Memory.State.GFs == null || (gf == GFs.Blank || gf == GFs.All)) return;
                    ITEM[0, 1] = new IGMDataItem.Integer { Data = Memory.State.GFs[gf].Level, Pos = new Rectangle(SIZE[0].X + 35, SIZE[0].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 6 };
                    ITEM[0, 2] = null;
                    ITEM[1, 1] = new IGMDataItem.Integer { Data = Memory.State.GFs[gf].CurrentHP(), Pos = new Rectangle(SIZE[1].X + 35, SIZE[1].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 6 };
                    ITEM[1, 3] = new IGMDataItem.Integer { Data = Memory.State.GFs[gf].MaxHP(), Pos = new Rectangle(SIZE[1].X + 155, SIZE[1].Y, 0, 0), Palette = 13, NumType = Icons.NumType.SysFntBig, Padding = 1, Spaces = 5 };
                }
            }

            private void ItemChangeEvent(object sender, KeyValuePair<ItemInMenu, FF8String> e) => _item = e.Key;

            private void TargetChangeEvent(object sender, Faces.ID e) => Fill(e);

            #endregion Methods
        }

        #endregion Classes
    }
}