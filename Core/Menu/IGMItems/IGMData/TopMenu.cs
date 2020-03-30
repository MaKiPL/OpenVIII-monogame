using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class IGMItems
    {
        #region Classes

        private class TopMenu : IGMData.Base
        {
            #region Fields

            private bool _eventSet;
            private FF8String[] _helpStr;
            private List<Action> _inputsOkayActions;

            //private int _averageWidth;
            private int _largestHeight;

            private int _largestWidth;

            //private int _totalWidth;
            private IReadOnlyDictionary<FF8String, FF8String> _pairs;

            private int[] _widths;

            #endregion Fields

            #region Properties

            private IReadOnlyList<FF8String> HelpStr => _helpStr;

            #endregion Properties

            #region Methods

            public static TopMenu Create(IReadOnlyDictionary<FF8String, FF8String> pairs)
            {
                var r = Create<TopMenu>();
                r._pairs = pairs;
                r.CreateData();
                return r;
            }

            public override bool Inputs_CANCEL()
            {
                base.Inputs_CANCEL();
                Menu.Module.State = MenuModule.Mode.IGM;
                IGM.Refresh();
                FadeIn();
                return true;
            }

            public override bool Inputs_OKAY()
            {
                if (CURSOR_SELECT >= _inputsOkayActions.Count) return false;
                base.Inputs_OKAY();
                _inputsOkayActions[CURSOR_SELECT]();
                return true;
            }

            public override void ModeChangeEvent(object sender, Enum e)
            {
                if (!e.Equals(Mode.TopMenu))
                    Cursor_Status |= Cursor_Status.Blinking;
                else
                    IGMItems.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<byte, FF8String>((byte)CURSOR_SELECT, HelpStr[CURSOR_SELECT]));
            }

            public override void Refresh()
            {
                if (!_eventSet && IGMItems != null)
                {
                    IGMItems.ModeChangeHandler += ModeChangeEvent;
                    _eventSet = true;
                }
                base.Refresh();
            }

            protected override void InitShift(int i, int col, int row)
            {
                SIZE[i].Inflate(0, (SIZE[i].Height - _largestHeight) / -2);
                SIZE[i].X += SIZE[i].Width / 2 - _widths[i] / 2;
                SIZE[i].Width = _widths[i];
                SIZE[i].Height = _largestHeight;
            }

            protected override void SetCursor_select(int value)
            {
                if (value != GetCursor_select())
                {
                    base.SetCursor_select(value);
                    IGMItems.ChoiceChangeHandler?.Invoke(this, new KeyValuePair<byte, FF8String>((byte)value, HelpStr[value]));
                }
            }

            private void CreateData()
            {
                _helpStr = new FF8String[_pairs.Count];
                _widths = new int[_pairs.Count];
                byte pos = 0;
                foreach (var pair in _pairs)
                {
                    _helpStr[pos] = pair.Value;
                    var rectangle = Memory.Font.RenderBasicText(pair.Key, 0, 0, skipdraw: true);
                    _widths[pos] = rectangle.Width;
                    if (rectangle.Width > _largestWidth) _largestWidth = rectangle.Width;
                    if (rectangle.Height > _largestHeight) _largestHeight = rectangle.Height;
                    //_totalWidth += rectangle.Width;

                    //_averageWidth = _totalWidth / pos;
                    ++pos;
                }
                Init(_pairs.Count, 1, new IGMDataItem.Box { Pos = new Rectangle(0, 12, 610, 54) }, _pairs.Count, 1);
                pos = 0;
                foreach (var pair in _pairs)
                {
                    ITEM[pos, 0] = new IGMDataItem.Text { Data = pair.Key, Pos = SIZE[pos] };
                    pos++;
                }
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status |= Cursor_Status.Horizontal;
                Cursor_Status |= Cursor_Status.Vertical;
                Cursor_Status |= Cursor_Status.Blinking;
                _inputsOkayActions = new List<Action>(Count)
                    {
                        Inputs_Okay_UseItem,
                        Inputs_Okay_Sort,
                        Inputs_Okay_Rearrange,
                        Inputs_Okay_BattleRearrange,
                    };
            }

            private static void Inputs_Okay_BattleRearrange()
            {
            }

            private static void Inputs_Okay_Rearrange()
            {
            }

            private static void Inputs_Okay_Sort()
            {
            }

            private static void Inputs_Okay_UseItem() => IGMItems?.SetMode(Mode.SelectItem);

            #endregion Methods
        }

        #endregion Classes
    }
}