using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class IGM
    {
        #region Classes

        [SuppressMessage("ReSharper", "NotAccessedField.Local")]
        private class IGMDataSideMenu : IGMData.Base
        {
            #region Fields

            private int _avgHeight;
            private int _avgWidth;
            private bool _eventSet;
            private FF8String[] _helpStr;
            private int _largestHeight;

            private int _largestWidth;

            private int _totalHeight;

            private int _totalWidth;

            private int[] _widths;

            #endregion Fields

            #region Methods

            public static IGMDataSideMenu Create(IReadOnlyDictionary<FF8String, FF8String> pairs)
            {
                var r = new IGMDataSideMenu
                {
                    _helpStr = new FF8String[pairs.Count],
                    _widths = new int[pairs.Count]
                };
                byte pos = 0;
                foreach (var pair in pairs)
                {
                    r._helpStr[pos] = pair.Value;
                    var rectangle = Memory.Font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                    r._widths[pos] = rectangle.Width;
                    if (rectangle.Width > r._largestWidth) r._largestWidth = rectangle.Width;
                    if (rectangle.Height > r._largestHeight) r._largestHeight = rectangle.Height;
                    r._totalWidth += rectangle.Width;
                    r._totalHeight += rectangle.Height;
                    r._avgWidth = r._totalWidth / (pos + 1);
                    r._avgHeight = r._totalHeight / (pos + 1);
                    pos++;
                }
                r.Init(pairs.Count, 1, new IGMDataItem.Box { Pos = new Rectangle { Width = 226, Height = 492, X = 843 - 226 } }, 1, pairs.Count);
                pos = 0;
                foreach (var pair in pairs)
                {
                    r.ITEM[pos, 0] = new IGMDataItem.Text { Data = pair.Key, Pos = new Rectangle(r.SIZE[pos].X, r.SIZE[pos].Y, 0, 0) };
                    pos++;
                }

                r.Cursor_Status |= (Cursor_Status.Enabled | Cursor_Status.Vertical | Cursor_Status.Horizontal);
                return r;
            }

            public override bool Inputs()
            {
                Cursor_Status &= ~Cursor_Status.Blinking;
                return base.Inputs();
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                FadeIn();
                Menu.Module.State = MenuModule.Mode.LoadGameChooseGame;
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch ((Items)CURSOR_SELECT)
                {
                    case Items.Junction:
                    case Items.Magic:
                    case Items.Status:
                        IGM.SetMode(Mode.ChooseChar);
                        return true;

                    case Items.Item:
                        Menu.Module.State = MenuModule.Mode.IGM_Items;
                        IGM_Items.Refresh();
                        return true;

                    case Items.Battle:
                        BattleMenus.CameFrom();
                        //Menu.Module.State = MenuModule.MainMenuStates.BattleMenu;
                        ModuleBattleDebug.ResetState();
                        Memory.Module = OpenVIII.Module.BattleDebug;
                        BattleMenus.Refresh();
                        FadeIn();
                        return true;
                    case Items.GF:
                        break;
                    case Items.Ability:
                        break;
                    case Items.Switch:
                        break;
                    case Items.Card:
                        break;
                    case Items.Config:
                        break;
                    case Items.Tutorial:
                        break;
                    case Items.Save:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return false;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.ChooseItem))
                {
                    Cursor_Status |= Cursor_Status.Blinking;
                }
            }

            public override void Refresh()
            {
                if (!_eventSet && IGM != null)
                {
                    IGM.ModeChangeHandler += ModeChangeEvent;
                    _eventSet = true;
                }
                IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)CURSOR_SELECT, _helpStr[CURSOR_SELECT]));
                base.Refresh();
            }

            protected override void InitShift(int i, int col, int row)
            {
                //SIZE[i].Inflate((SIZE[i].Width - largestWidth) / -2, 0); // center
                SIZE[i].Inflate(-20, 0);
                SIZE[i].Y += SIZE[i].Height / 2 - _largestHeight / 2 + 5;
                //SIZE[i].Width = largestWidth;
                SIZE[i].Height = _largestHeight;
            }

            protected override void SetCursor_select(int value)
            {
                if (!value.Equals(GetCursor_select()))
                {
                    base.SetCursor_select(value);
                    IGM?.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<Items, FF8String>((Items)value, _helpStr[value]));
                }
            }

            [SuppressMessage("ReSharper", "UnusedParameter.Local")]
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            private void ChoiceChangeEvent(object _, FF8String e) => ((IGMDataItem.Box)CONTAINER).Data = e;

            #endregion Methods
        }

        #endregion Classes
    }
}