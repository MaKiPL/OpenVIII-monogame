using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenVIII.IGMData
{
    public class DebugSelectPool<DataType> : Pool.Base<IEnumerable<DataType>, DataType>
    {
        #region Fields

        private bool skipRefresh = true;
        private Func<DataType, bool> OkayFunc;

        #endregion Fields

        #region Methods

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
        }

        public static DebugSelectPool<inDataType> Create<inDataType>(Rectangle pos, IEnumerable<inDataType> source, Func<inDataType, bool>  OkayFunc, Action<string> FilterAction)
        {
            DebugSelectPool<inDataType> r = Base.Create<DebugSelectPool<inDataType>>(45, 1, new IGMDataItem.Box { Pos = pos }, 3, 15);
            r.Source = source;
            r.OkayFunc = OkayFunc;
            r.FilterAction = FilterAction;
            return r;
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

        public override void Refresh()
        {
            if (Source == null || skipRefresh)
            {
                skipRefresh = false;
                return;
            }
            int total = Rows * Cols;
            DefaultPages = Source.Count() / total;
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
            for(; p<total;p++)
            {
                Contents[p] = default;
                BLANKS[p] = true;
                ITEM[p, 0].Hide();
            }

            base.Refresh();
        }

        private int Col => (CURSOR_SELECT / Rows);

        private int Row => (CURSOR_SELECT / Cols);

        public override void Inputs_Left()
        {
            if (Input2.Button(MouseButtons.MouseWheelup) ||Col == 0)
            {
                PAGE_PREV();
                Refresh();
            }
            else
                CURSOR_SELECT -= Rows;
            base.Inputs_Left();
        }
        string filter;
        private Action<string> FilterAction;

        public override bool Inputs()
        {
            if (InputKeyboard.State.GetPressedKeys().Any(x=>(int)x >= (int)Keys.D0 && (int)x <= (int)Keys.Z ))
                return false;
            else
                return base.Inputs();
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
        public override bool Inputs_CANCEL()
        {
            if (filter.Length == 0)
            {
                base.Inputs_CANCEL();
                Hide();
                return true;
            }
            if (filter.Length > 1)
                filter = filter.Substring(0, filter.Length - 1).Trim();
            else
                filter = "";
            if (!string.IsNullOrWhiteSpace(filter))
                FilterAction?.Invoke(filter);
            //Debug.WriteLine(filter);
            return false;
        }
        ~DebugSelectPool()
            { Game1.onTextEntered -= Game1_onTextEntered; }
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
            Cursor_Status |= Cursor_Status.Horizontal;
            Hide();
        }
        public void Refresh(IEnumerable<DataType> src)
        {
            Source = src;
            Refresh();
        }
        private void Game1_onTextEntered(object sender, TextInputEventArgs e)
        {
            if (filter != null && filter.Length < 10 && Enabled)
            {
                filter += e.Character;
                filter=filter.Trim().TrimEnd('\b');
                if (!string.IsNullOrWhiteSpace(filter))
                    FilterAction?.Invoke(filter);
                //Debug.WriteLine(filter);
            }
        }

        #endregion Methods
    }
}