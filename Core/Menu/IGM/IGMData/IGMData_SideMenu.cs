using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        private class IGMData_SideMenu : IGMData.Base
        {
            #region Fields

            private FF8String[] _helpStr;
            private int avgheight;
            private int avgwidth;
            private bool eventSet = false;

            private int largestheight;

            private int largestwidth;

            private int totalheight;

            private int totalwidth;

            private int[] widths;

            #endregion Fields

            #region Constructors

            public static IGMData_SideMenu Create(IReadOnlyDictionary<FF8String, FF8String> pairs)
            {
                IGMData_SideMenu r = new IGMData_SideMenu
                {
                    _helpStr = new FF8String[pairs.Count],
                    widths = new int[pairs.Count]
                };
                byte pos = 0;
                foreach (KeyValuePair<FF8String, FF8String> pair in pairs)
                {
                    r._helpStr[pos] = pair.Value;
                    Rectangle rectangle = Memory.font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                    r.widths[pos] = rectangle.Width;
                    if (rectangle.Width > r.largestwidth) r.largestwidth = rectangle.Width;
                    if (rectangle.Height > r.largestheight) r.largestheight = rectangle.Height;
                    r.totalwidth += rectangle.Width;
                    r.totalheight += rectangle.Height;
                    r.avgwidth = r.totalwidth / (pos+1);
                    r.avgheight = r.totalheight / (pos+1);
                    pos++;
                }
                r.Init(pairs.Count, 1, new IGMDataItem.Box(pos: new Rectangle { Width = 226, Height = 492, X = 843 - 226 }), 1, pairs.Count);
                pos = 0;
                foreach (KeyValuePair<FF8String, FF8String> pair in pairs)
                {
                    r.ITEM[pos, 0] = new IGMDataItem.Text { Data = pair.Key, Pos = new Rectangle(r.SIZE[pos].X, r.SIZE[pos].Y, 0, 0) };
                    pos++;
                }

                r.Cursor_Status |= (Cursor_Status.Enabled | Cursor_Status.Vertical | Cursor_Status.Horizontal);
                return r;
            }

            #endregion Constructors

            #region Methods

            public override bool Inputs()
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                return base.Inputs();
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                FadeIn();
                Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.LoadGameChooseGame;
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                switch ((Items)CURSOR_SELECT)
                {
                    case Items.Junction:
                    case Items.Magic:
                    case Items.Status:
                        IGM.SetMode(Mode.ChooseChar);
                        return true;

                    case Items.Item:
                        Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.IGM_Items;
                        IGM_Items.Refresh();
                        return true;

                    case Items.Battle:
                        BattleMenus.CameFrom();
                        //Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.BattleMenu;
                        Memory.module = MODULE.BATTLE_DEBUG;
                        BattleMenus.Refresh();
                        FadeIn();
                        return true;
                }
                return false;
            }

            public override void Refresh()
            {
                if (!eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    eventSet = true;
                }
                IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)CURSOR_SELECT, _helpStr[CURSOR_SELECT]));
                base.Refresh();
            }

            protected override void InitShift(int i, int col, int row)
            {
                //SIZE[i].Inflate((SIZE[i].Width - largestwidth) / -2, 0); // center
                SIZE[i].Inflate(-20, 0);
                SIZE[i].Y += SIZE[i].Height / 2 - largestheight / 2 + 5;
                //SIZE[i].Width = largestwidth;
                SIZE[i].Height = largestheight;
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.ChooseItem))
                {
                    Cursor_Status |= Cursor_Status.Blinking;
                }
            }

            protected override void SetCursor_select(int value)
            {
                if (!value.Equals(GetCursor_select()))
                {
                    base.SetCursor_select(value);
                    IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)value, _helpStr[value]));
                }
            }

            private void ChoiceChangeEvent(object sender, FF8String e) => ((IGMDataItem.Box)CONTAINER).Data = e;

            #endregion Methods
        }

        #endregion Classes
    }
}