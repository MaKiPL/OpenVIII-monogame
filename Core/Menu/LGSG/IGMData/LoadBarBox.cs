using System;
using Microsoft.Xna.Framework;
using static OpenVIII.Kernel_bin;

namespace OpenVIII.IGMData
{
    public class LoadBarBox : IGMData.Base
    {
        public bool Save { get; private set; }
        public bool Slot { get; private set; }
        public int TotalWidth { get; private set; }
        public Slide<float> LoadBarSlide { get; private set; }
        #region Methods

        public static LoadBarBox Create(Rectangle pos)
        {
            return Create<LoadBarBox>(1, 2, container: new IGMDataItem.Box { Pos = pos, Title = Icons.ID.INFO });
        }

        protected override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(IGM_LGSG.Mode))
            {
                Save = e.HasFlag(IGM_LGSG.Mode.Save);
                Slot = e.HasFlag(IGM_LGSG.Mode.Slot);
                if (e.HasFlag(IGM_LGSG.Mode.Checking))
                {
                    Show();
                    Refresh();
                }
                else
                    Hide();
            }
        }
        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Height = 24;
            SIZE[i].Inflate(-10, 0);
            SIZE[i].Y += Height / 2 - SIZE[i].Height / 2;
        }

        protected override void Init()
        {
            base.Init();
            

            ITEM[0, 0] = new IGMDataItem.Icon
            {
                Data = Icons.ID.Bar_BG,
                Pos = SIZE[0]
            };
            var r = ITEM[0, 0].Pos;
            //r.Offset(0, 1 * Menu.TextScale.Y);
            //r.Inflate(-4, 0);
            TotalWidth = r.Width;
            LoadBarSlide = new Slide<float>(0, TotalWidth, 1000d, MathHelper.SmoothStep);
            r.Width = 0;
            ITEM[0, 1] = new IGMDataItem.Icon { Data = Icons.ID.Bar_Fill, Pos =r };
            Cursor_Status |= Cursor_Status.Enabled | Cursor_Status.Hidden | Cursor_Status.Static;
        }
        IGMDataItem.Icon RedBar { get => (IGMDataItem.Icon)ITEM[0, 1]; set => ITEM[0, 1] = value; }
        public override void Refresh()
        {
            if (Enabled)
            {
                base.Refresh();
                LoadBarSlide.Restart();
            }
        }

        public override bool Update()
        {
            if (Enabled)
            {
                base.Update();

                if (!LoadBarSlide.Done)
                {
                    var r = RedBar.Pos;
                    r.Width = (int)LoadBarSlide.Update();
                    RedBar.Pos = r;
                }
                else
                {
                    Inputs_OKAY();
                }
                return true;
            }
            return false;
        }
        public override bool Inputs_OKAY()
        {

            init_debugger_Audio.PlaySound(35);
            skipsnd = true;
            base.Inputs_OKAY();
            if (Slot)
            {
                var mode = IGM_LGSG.Mode.Game |
                        IGM_LGSG.Mode.Choose |
                        (Save ? IGM_LGSG.Mode.Save : IGM_LGSG.Mode.Nothing);

                if (CURSOR_SELECT == 0)
                    Menu.IGM_LGSG.SetMode(mode | IGM_LGSG.Mode.Slot1);
                else if (CURSOR_SELECT == 1)
                    Menu.IGM_LGSG.SetMode(mode | IGM_LGSG.Mode.Slot1);
            }
            //else load game or save game.
            return true;
        }
        #endregion Methods
    }
}