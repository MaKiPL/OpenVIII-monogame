using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenVIII.IGMData.Pool
{
    public partial class Draw
    {
        #region Classes

        /// <summary>
        /// (Draw) or (Cast and display target window)
        /// </summary>
        protected class Commands : IGMData.Base
        {
            #region Fields

            public Dictionary<int, Func<bool>> OKAY_Actions;
            private Battle.Dat.Magic Magic;

            #endregion Fields

            #region Properties

            public int _Draw => 0;

            public int Cast => 1;

            public IGMData.Target.Group Target_Group => (IGMData.Target.Group)(((IGMData.Base)ITEM[Targets_Window, 0]));

            public int Targets_Window => Count - 1;

            #endregion Properties

            #region Methods

            public static Commands Create(Rectangle pos, Damageable damageable, bool battle = false) => Create<Commands>(3, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.CHOICE }, 1, 2, damageable);

            public override void HideChildren()
            {
                if (Enabled)
                {
                    //base.Hide();
                    //maybe overkill to run hide on items. if group is hidden it won't draw.
                    if (!skipdata)
                    {
                        int pos = 0;
                        foreach (Menu_Base i in ITEM)
                        {
                            if (pos != _Draw && pos != Cast && i != null)
                            {
                                i.HideChildren();
                                i.Hide();
                            }
                            else i?.HideChildren();
                        }
                    }
                }
            }

            public override bool Inputs()
            {
                if (Target_Group.Enabled)
                {
                    Cursor_Status |= Cursor_Status.Blinking;
                    return Target_Group.Inputs();
                }
                else
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;
                    return base.Inputs();
                }
            }

            public override bool Inputs_CANCEL()
            {
                Hide();
                return true;
            }

            public override bool Inputs_OKAY()
            {
                bool ret = false;
                if (OKAY_Actions.TryGetValue(CURSOR_SELECT, out Func<bool> func))
                    ret = func();
                if (ret)
                    base.Inputs_OKAY();
                return ret;
            }

            public override void Refresh()
            {
                if (Magic.ID > 0)
                {
                    bool gf = Magic.GF != GFs.Blank;
                    bool full = (Damageable.GetCharacterData(out Saves.CharacterData c) && c.Magics.TryGetByKey(Magic.ID, out byte qty) && qty < 100);
                    //TODO check for empty magic slots. as can only have 30 something spells in inventory.

                    bool candraw = gf || !full;
                    if (!candraw)
                    {
                        ((IGMDataItem.Text)ITEM[_Draw, 0]).FontColor = Font.ColorID.Dark_Grey;
                        BLANKS[_Draw] = true;
                    }
                    else
                    {
                        ((IGMDataItem.Text)ITEM[_Draw, 0]).FontColor = Font.ColorID.White;
                        BLANKS[_Draw] = false;
                    }
                    if (gf)
                    {
                        ((IGMDataItem.Text)ITEM[Cast, 0]).FontColor = Font.ColorID.Dark_Grey;
                        BLANKS[_Draw] = true;
                    }
                    else
                    {
                        Target_Group.SelectTargetWindows(Magic.DATA);
                        ((IGMDataItem.Text)ITEM[Cast, 0]).FontColor = Font.ColorID.White;
                        BLANKS[_Draw] = false;
                    }
                }
                base.Refresh();
            }

            public void Refresh(Battle.Dat.Magic magic)
            {
                if (Magic.ID != magic.ID)
                {
                    Magic = magic;
                    Refresh();
                }
            }

            protected override void Init()
            {
                base.Init();
                ITEM[_Draw, 0] = new IGMDataItem.Text { Data = Memory.Strings.Read(Strings.FileID.Kernel, 0, 12), Pos = SIZE[_Draw] };
                ITEM[Cast, 0] = new IGMDataItem.Text { Data = Memory.Strings.Read(Strings.FileID.Kernel, 0, 18), Pos = SIZE[Cast] };
                ITEM[Targets_Window, 0] = IGMData.Target.Group.Create(Damageable, false);
                Cursor_Status = Cursor_Status.Enabled;
                OKAY_Actions = new Dictionary<int, Func<bool>>
            {
                {_Draw, Inputs_OKAY_Draw },
                {Cast, Inputs_OKAY_Cast },
            };
                PointerZIndex = 0;
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                //SIZE[i].Inflate(-18, -20);
                //SIZE[i].Y -= 5 * row;
                SIZE[i].Inflate(-22, -8);
                SIZE[i].Offset(0, 12 + (-8 * row));
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            private bool Inputs_OKAY_Cast()
            {
                Debug.WriteLine($"{Damageable.Name} Casting {Magic.Name}({Magic.ID}) from enemy.");
                Target_Group.ShowTargetWindows();
                return true;
            }

            private bool Inputs_OKAY_Draw()
            {
                Debug.WriteLine($"{Damageable.Name} Drawing {Magic.Name}({Magic.ID}) from enemy.");
                Damageable.EndTurn();
                return true;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}