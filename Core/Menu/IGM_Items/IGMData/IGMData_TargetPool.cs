using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM_Items
    {
        #region Classes

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Using Faces.BattleID because it contains characters and gfs. Can cast to Characters or subtract
        /// 16 and cast to GFs
        /// </remarks>
        private class IGMData_TargetPool : IGMData.Pool.Base<Saves.Data, Faces.ID>
        {
            #region Fields

            private bool eventSet;
            private bool ForceRefresh;

            #endregion Fields

            #region Properties

            private bool All => (Item.Target & (Item_In_Menu._Target.All | Item_In_Menu._Target.All2)) != 0;

            public Item_In_Menu Item { get; private set; }

            public byte TopMenuChoice { get; private set; }

            private bool IsMe => IGM_Items.GetMode().Equals(Mode.UseItemOnTarget);

            #endregion Properties

            #region Methods

            public static IGMData_TargetPool Create()
            {
                IGMData_TargetPool r = Create<IGMData_TargetPool>(9, 3, new IGMDataItem.Box { Pos = new Rectangle(420, 150, 420, 360), Title = Icons.ID.TARGET }, 9, 1);
                r.Cursor_Status &= ~Cursor_Status.Enabled;
                return r;
            }

            public override void Draw()
            {
                if (All && IsMe)
                    Cursor_Status &= ~Cursor_Status.Enabled;
                base.Draw();
                if (All && IsMe)
                {
                    // if all draw blinking pointers on everyone.
                    byte i = 0;
                    foreach (Point c in CURSOR)
                    {
                        if (!BLANKS[i] && ITEM[i, 0] != null && ITEM[i, 0].Enabled && c != Point.Zero)
                            DrawPointer(c, blink: true);
                        i++;
                    }
                    Cursor_Status |= Cursor_Status.Enabled;
                }
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
                IGM_Items.SetMode(Mode.SelectItem);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                bool ret = false;
                if (All)
                    ret = Item.Use(Faces.ID.Blank);
                else if (!BLANKS[CURSOR_SELECT])
                    ret = Item.Use(Contents[CURSOR_SELECT]);
                if (ret)
                {
                    base.Inputs_OKAY();
                    Fill();
                    IGM_Items.Refresh(true);
                    return true;
                }
                return false;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!IsMe)
                    Cursor_Status &= ~Cursor_Status.Enabled;
                else
                    IGM_Items.TargetChangeHandler?.Invoke(this, Contents[CURSOR_SELECT]);
            }

            public override void Refresh()
            {
                if (!eventSet && IGM_Items != null)
                {
                    IGM_Items.ModeChangeHandler += ModeChangeEvent;
                    IGM_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                    IGM_Items.ItemPool.ItemChangeHandler += ItemTypeChangeEvent;
                    eventSet = true;
                }
                ////List won't populate unless theres a valid item set.
                //if (Item.Type == Item_In_Menu._Type.None && Memory.State.Items != null)
                //    Item = Memory.MItems[Memory.State.Items.FirstOrDefault(m => m.BattleID > 0 && m.QTY > 0).BattleID];
                else
                    Fill();
                ForceRefresh = true;
                base.Refresh();
            }

            public override void ResetPages()
            {
            }

            protected override void Init()
            {
                base.Init();
                for (int i = 0; i < Rows; i++)
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
                if (Pages > 1)
                {
                    base.PAGE_NEXT();
                    Fill();
                    base.Refresh();
                }
            }

            protected override void PAGE_PREV()
            {
                if (Pages > 1)
                {
                    base.PAGE_PREV();
                    Fill();
                    base.Refresh();
                }
            }

            protected override void SetCursor_select(int value)
            {
                if (!value.Equals(GetCursor_select()))
                {
                    base.SetCursor_select(value);
                    IGM_Items.TargetChangeHandler?.Invoke(this, Contents[CURSOR_SELECT]);
                }
            }

            private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e) => TopMenuChoice = e.Key;

            private void Fill()
            {
                Faces.ID id = 0;
                int skip = Page * Rows;
                for (int i = 0; i < Rows; i++)
                {
                    bool gftest = false;
                    bool ctest = false;
                    Characters character = Characters.Blank;
                    GFs gf = GFs.Blank;
                    while (!Enum.IsDefined(typeof(Faces.ID), id)
                        || !((ctest = Item.TestCharacter(ref id, out character)) || (gftest = Item.TestGF(ref id, out gf)))
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
                    int hp = (ctest || gftest) ? Memory.State[id]?.CurrentHP() ?? -1 : -1;
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

            private void ItemTypeChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e)
            {
                CURSOR_SELECT = 0;
                if (!Item.Equals(e.Key) || Page > 0)
                {
                    Page = 0;
                    bool sameTargets = Item.Target != e.Key.Target || Item.Type != e.Key.Type;
                    if (!sameTargets)
                    {
                        sameTargets = (Item.Type == Item_In_Menu._Type.GF_Learn && Item.Learn != e.Key.Learn);
                        sameTargets = sameTargets || (Item.Type == Item_In_Menu._Type.Blue_Magic && Item.Learned_Blue_Magic != e.Key.Learned_Blue_Magic);
                    }
                    Item = e.Key;
                    if (sameTargets || ForceRefresh)
                    {
                        Fill();
                        base.Refresh();
                        ForceRefresh = false;
                    }
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}