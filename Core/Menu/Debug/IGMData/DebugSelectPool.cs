using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OpenVIII.IGMData
{
    public class DebugSelectPool<DataType> : Pool.Base<IEnumerable<DataType>, DataType>
    {
        #region Fields

        private string filter;
        private Action<string> FilterAction;
        private Func<DataType, bool> OkayFunc;
        private bool skipRefresh = true;

        #endregion Fields

        #region Destructors

        ~DebugSelectPool()
        { Game1.onTextEntered -= Game1_onTextEntered; }

        #endregion Destructors

        #region Properties

        private int Col => (CURSOR_SELECT / Rows);

        private int Row => (CURSOR_SELECT / Cols);

        #endregion Properties

        #region Methods

        public static DebugSelectPool<inDataType> Create<inDataType>(Rectangle pos, IEnumerable<inDataType> source, Func<inDataType, bool> OkayFunc, Action<string> FilterAction, int Cols = 3, int Rows = 12)
        {
            DebugSelectPool<inDataType> r = Base.Create<DebugSelectPool<inDataType>>(Rows*Cols+1, 1, new IGMDataItem.Box { Pos = pos }, Cols, Rows);
            r.Source = source;
            r.OkayFunc = OkayFunc;
            r.FilterAction = FilterAction;
            return r;
        }

        public override bool Inputs()
        {
            if (InputKeyboard.State.GetPressedKeys().Any(x => (int)x >= (int)Keys.D0 && (int)x <= (int)Keys.Z))
                return false;
            else if (!string.IsNullOrWhiteSpace(filter) && Input2.DelayedButton(FF8TextTagKey.Cancel, ButtonTrigger.Press | ButtonTrigger.Force))
            {
                Inputs_CANCEL();
                return true;
            }
            else
                return base.Inputs();
        }

        public override bool Inputs_CANCEL()
        {
            if (filter.Length == 0)
            {
                base.Inputs_CANCEL();
                Hide();
                return true;
            }
            if (!string.IsNullOrWhiteSpace(filter))
            {
                if (filter.Length > 1)
                    filter = filter.Substring(0, filter.Length - 1).Trim();
                else
                    filter = "";
                ((IGMDataItem.Box)ITEM[Count - 3, 0]).Data = filter;
                FilterAction?.Invoke(filter);
            }

            //Debug.WriteLine(filter);
            return false;
        }

        public override void Inputs_Left()
        {
            if (Input2.Button(MouseButtons.MouseWheelup) || Col == 0)
            {
                PAGE_PREV();
                Refresh();
            }
            else
                CURSOR_SELECT -= Rows;
            base.Inputs_Left();
        }

        public override bool Inputs_OKAY()
        {
            if (!BLANKS[CURSOR_SELECT] && OkayFunc.Invoke(Contents[CURSOR_SELECT]))
            {
                base.Inputs_OKAY();
                return true;
            }
            return false;
        }

        public override void Inputs_Right()
        {
            if (Input2.Button(MouseButtons.MouseWheeldown) || Col == Cols - 1)
            {
                PAGE_NEXT();
                Refresh();
            }
            else
                CURSOR_SELECT += Rows;
            base.Inputs_Right();
        }

        public override void Refresh()
        {
            if (Source == null || skipRefresh)
            {
                skipRefresh = false;
                return;
            }
            int total = Rows * Cols;
            DefaultPages = Source.Count() / total;
            if (DefaultPages <= Page)
                Page = DefaultPages - 1;
            int skip = Page * total;
            int p = 0;
            foreach (DataType i in Source)
            {
                if (skip > 0)
                {
                    skip--;
                    continue;
                }
                if (p >= total)
                    break;
                Contents[p] = i;
                Menu_Base menu_Base = ITEM[p, 0];
                menu_Base.Show();
                IGMDataItem.Text text = (IGMDataItem.Text)menu_Base;
                text.Data = new FF8String(i.ToString());
                BLANKS[p] = false;
                p++;
            }
            for (; p < total; p++)
            {
                Contents[p] = default;
                BLANKS[p] = true;
                ITEM[p, 0].Hide();
            }

            base.Refresh();
        }

        public void Refresh(IEnumerable<DataType> src)
        {
            Source = src;
            Refresh();
        }

        protected override void Init()
        {
            base.Init();
            filter = "";
            Game1.onTextEntered += Game1_onTextEntered;
            Contents = new DataType[Rows * Cols];
            foreach (int i in Enumerable.Range(0, Rows * Cols))
            {
                ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
            }
            Rectangle rect = new Rectangle(CONTAINER.X + CONTAINER.Width / 4, CONTAINER.Y - 112, CONTAINER.Width / 2, 120);
            ITEM[Count - 3, 0] = new IGMDataItem.Box { Pos = rect, Options = Box_Options.Center | Box_Options.Middle, Title = Icons.ID.INFO };
            Cursor_Status |= Cursor_Status.Horizontal;
            Hide();
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(-4 * col, 12 + (-4 * row));
        }

        private void Game1_onTextEntered(object sender, TextInputEventArgs e)
        {
            if (filter != null && filter.Length < 10 && Enabled)
            {
                filter += e.Character;
                filter = filter.Trim().TrimEnd('\b');
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    ((IGMDataItem.Box)ITEM[Count - 3, 0]).Data = filter;
                    FilterAction?.Invoke(filter);
                }
                //Debug.WriteLine(filter);
            }
        }

        #endregion Methods
    }
}