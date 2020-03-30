using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGMItems
    {
        #region Classes

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Using Faces.ID because it contains characters and gfs. Can cast to Characters or subtract
        /// 16 and cast to GFs
        /// </remarks>
        private class TargetPool : IGMData.Pool.Base<Saves.Data, Faces.ID>
        {
            #region Fields

            private bool _eventSet;
            private bool _forceRefresh;

            #endregion Fields

            #region Properties

            private bool All => (Item.ItemTarget & (ItemTarget.All | ItemTarget.All2)) != 0;

            private ItemInMenu Item { get; set; }

            private static bool IsMe => IGMItems.GetMode().Equals(Mode.UseItemOnTarget);

            #endregion Properties

            #region Methods

            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static TargetPool Create()
            {
                var r = Create<TargetPool>(9, 3, new IGMDataItem.Box { Pos = new Rectangle(420, 150, 420, 360), Title = Icons.ID.TARGET }, 9, 1);
                r.Cursor_Status &= ~Cursor_Status.Enabled;
                return r;
            }

            public override void Draw()
            {
                if (All && IsMe)
                    Cursor_Status &= ~Cursor_Status.Enabled;
                base.Draw();
                if (!All || !IsMe) return;
                // if all draw blinking pointers on everyone.
                byte i = 0;
                foreach (var c in CURSOR)
                {
                    if (!BLANKS[i] && ITEM[i, 0] != null && ITEM[i, 0].Enabled && c != Point.Zero)
                        DrawPointer(c, blink: true);
                    i++;
                }
                Cursor_Status |= Cursor_Status.Enabled;
            }

            public override bool Inputs()
            {
                // if ((Item.Target & (Item_In_Menu._Target.All|Item_In_Menu._Target.All2)) == 0)
                Cursor_Status |= Cursor_Status.Enabled;
                return base.Inputs();
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                IGMItems.SetMode(Mode.SelectItem);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                var ret = false;
                if (All)
                    ret = Item.Use(Faces.ID.Blank);
                else if (!BLANKS[CURSOR_SELECT])
                    ret = Item.Use(Contents[CURSOR_SELECT]);
                if (ret)
                {
                    base.Inputs_OKAY();
                    Fill();
                    IGMItems.Refresh(true);
                    return true;
                }
                return false;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!IsMe)
                    Cursor_Status &= ~Cursor_Status.Enabled;
                else
                    IGMItems.TargetChangeHandler?.Invoke(this, Contents[CURSOR_SELECT]);
            }

            public override void Refresh()
            {
                if (!_eventSet && IGMItems != null)
                {
                    IGMItems.ModeChangeHandler += ModeChangeEvent;
                    IGMItems.ChoiceChangeHandler += ChoiceChangeEvent;
                    IGMItems.ItemPool.ItemChangeHandler += ItemTypeChangeEvent;
                    _eventSet = true;
                }
                ////List won't populate unless theres a valid item set.
                //if (Item.Type == Item_In_Menu._Type.None && Memory.State.Items != null)
                //    Item = Memory.MItems[Memory.State.Items.FirstOrDefault(m => m.ID > 0 && m.QTY > 0).ID];
                else
                    Fill();
                _forceRefresh = true;
                base.Refresh();
            }

            public override void ResetPages()
            {
            }

            protected override void Init()
            {
                base.Init();
                for (var i = 0; i < Rows; i++)
                {
                    ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
                    ITEM[i, 0].Hide();
                    ITEM[i, 1] = new IGMDataItem.Icon { Data = Icons.ID.HP2, Pos = new Rectangle(SIZE[i].X + SIZE[i].Width - (20 * 7), SIZE[i].Y, 0, 0), Palette = 13 };
                    ITEM[i, 1].Hide();
                    ITEM[i, 2] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[i].X + SIZE[i].Width - (20 * 4), SIZE[i].Y, 0, 0), Spaces = 4 };
                    ITEM[i, 2].Hide();
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 3 * row;
                //SIZE[i].X += 2;
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            protected override void PAGE_NEXT()
            {
                if (Pages <= 1) return;
                base.PAGE_NEXT();
                Fill();
                base.Refresh();
            }

            protected override void PAGE_PREV()
            {
                if (Pages <= 1) return;
                base.PAGE_PREV();
                Fill();
                base.Refresh();
            }

            protected override void SetCursor_select(int value)
            {
                if (value.Equals(GetCursor_select())) return;
                base.SetCursor_select(value);
                IGMItems.TargetChangeHandler?.Invoke(this, Contents[CURSOR_SELECT]);
            }

            private static void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e)
            {
            }

            private void Fill()
            {
                Faces.ID id = 0;
                var skip = Page * Rows;
                for (var i = 0; i < Rows; i++)
                {
                    while (!Enum.IsDefined(typeof(Faces.ID), id)
                           || !((Item.TestCharacter(ref id, out _)) || (Item.TestGF(ref id, out _)))
                           || skip-- > 0)
                        if ((byte)++id > 32)
                        {
                            for (; i < Rows; i++)
                            {
                                ITEM[i, 0].Hide();
                                ITEM[i, 1].Hide();
                                ITEM[i, 2].Hide();
                                BLANKS[i] = true;
                                Contents[i] = Faces.ID.Blank;
                            }
                            //Pages = Page + 1;
                            return;
                        }
                    ((IGMDataItem.Text)ITEM[i, 0]).Data = Memory.Strings.GetName(id);
                    ITEM[i, 0].Show();
                    var hp = Memory.State[id]?.CurrentHP() ?? -1;
                    BLANKS[i] = false;
                    Contents[i] = id;
                    if (hp > -1)
                    {
                        ((IGMDataItem.Integer)ITEM[i, 2]).Data = hp;
                        ITEM[i, 1].Show();
                        ITEM[i, 2].Show();
                    }
                    else
                    {
                        ITEM[i, 1].Hide();
                        ITEM[i, 2].Hide();
                    }

                    id++;
                }
                //Pages = Page + 2;
            }

            private void ItemTypeChangeEvent(object sender, KeyValuePair<ItemInMenu, FF8String> e)
            {
                CURSOR_SELECT = 0;
                if (Item.Equals(e.Key) && Page <= 0) return;
                Page = 0;
                var sameTargets = Item.ItemTarget != e.Key.ItemTarget || Item.ItemType != e.Key.ItemType;
                if (!sameTargets)
                {
                    sameTargets = (Item.ItemType == ItemType.GFLearn && Item.Learn != e.Key.Learn);
                    sameTargets = sameTargets || (Item.ItemType == ItemType.BlueMagic && Item.LearnedBlueMagic != e.Key.LearnedBlueMagic);
                }
                Item = e.Key;
                if (!sameTargets && !_forceRefresh) return;
                Fill();
                base.Refresh();
                _forceRefresh = false;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}