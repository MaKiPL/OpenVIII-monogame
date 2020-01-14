using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Currently a menu for field screen to enable or disable elements or test scripts.
    /// </summary>
    public class FieldMenu : Menu
    {
        public static FieldMenu Create() => Create<FieldMenu>();
        #region Enums

        private enum Mode
        {
            On
        }

        #endregion Enums

        #region Methods

        protected override void Init()
        {
            //Size = new Vector2(960f, 720f);
            Size = new Vector2(1280f, 720f);
            base.Init();
            Data[Mode.On] = IGMData.FieldDebugControls.Create(new Rectangle(0, 0, 360, 360));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.On);
        }
        public override bool Inputs()
        {
            bool r = false;
            switch((Mode)GetMode())
            {
                case Mode.On:
                    r = Data[GetMode()].Inputs() || r;
                    break;
            }
            r = base.Inputs() || r;

            return r;
        }

        #endregion Methods
    }
}

namespace OpenVIII.Fields.IGMData
{
    public class FieldDebugControls : OpenVIII.IGMData.Base
    {
        #region Properties

        public IGMDataItem.Text FieldName { get => (IGMDataItem.Text)ITEM[0, 0]; protected set => ITEM[0, 0] = value; }

        #endregion Properties

        #region Methods

        public static FieldDebugControls Create(Rectangle pos) => Create<FieldDebugControls>(4, 1, new IGMDataItem.Box { Pos = pos }, 1, 4);
        public override void Refresh()
        {
            FieldName.Data = $"{ Memory.FieldHolder.FieldID} - { Memory.FieldHolder.GetString()}";
            BLANKS[0] = false;
            base.Refresh();
        }

        protected override void Init()
        {
            base.Init();
            foreach (int i in Enumerable.Range(0,Count))
            {
                ITEM[i, 0] = new IGMDataItem.Text { Pos = SIZE[i] };
            }
            Cursor_Status = Cursor_Status.Enabled;
        }
        public override void Inputs_Left()
        {
            if (CURSOR_SELECT == 0)
            {
                if (Memory.FieldHolder.FieldID >0)
                    Memory.FieldHolder.FieldID--;
                else
                    Memory.FieldHolder.FieldID = checked((ushort)(Memory.FieldHolder.fields.Length - 1));
                Module.ResetField();
            }
            base.Inputs_Left();
        }
        public override void Inputs_Right()
        {
            if (CURSOR_SELECT == 0)
            {
                if (Memory.FieldHolder.FieldID < checked((ushort)(Memory.FieldHolder.fields.Length - 1)))
                    Memory.FieldHolder.FieldID++;
                else
                    Memory.FieldHolder.FieldID = 0;
                Module.ResetField();
            }
            base.Inputs_Right();
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        #endregion Methods
    }
}