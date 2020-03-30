using Microsoft.Xna.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OpenVIII.IGMData
{
    public class LoadBarBox : Base
    {
        #region Properties

        public Slide<float> LoadBarSlide { get; private set; }
        public bool Save { get; private set; }
        public bool Slot { get; private set; }
        public int TotalWidth { get; private set; }
        private static TimeSpan Time => TimeSpan.FromMilliseconds(1000d);

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private IGMDataItem.Icon RedBar { get => (IGMDataItem.Icon)ITEM[0, 1]; set => ITEM[0, 1] = value; }

        #endregion Properties

        #region Methods

        public static LoadBarBox Create(Rectangle pos) => Create<LoadBarBox>(1, 2, container: new IGMDataItem.Box { Pos = pos, Title = Icons.ID.INFO });

        public override bool Inputs_OKAY()
        {
            if (Slot)
            {
                setMode(IGMLoadSaveGame.Mode.Game |
                        IGMLoadSaveGame.Mode.Choose |
                        (Save ? IGMLoadSaveGame.Mode.Save : IGMLoadSaveGame.Mode.Nothing));
                playSound(35);
                return true;
            }
            else
            {
                Menu.FadeIn();
                Menu.Module.State = MenuModule.Mode.IGM;
                Menu.IGM.Refresh();

                //TODO if save ask if you are sure if you are replacing an existing save.
                setMode(IGMLoadSaveGame.Mode.Game |
                        IGMLoadSaveGame.Mode.Choose |
                        (Save ? IGMLoadSaveGame.Mode.Save : IGMLoadSaveGame.Mode.Nothing));
                playSound(36);
                return true;
            }
            //return false;
            void playSound(int snd)
            {
                AV.Sound.Play(snd);
                skipsnd = true;
                base.Inputs_OKAY();
            }
            void setMode(IGMLoadSaveGame.Mode mode)
            {
                if (Menu.IGMLoadSaveGame.GetMode().HasFlag(IGMLoadSaveGame.Mode.Slot1))
                    Menu.IGMLoadSaveGame.SetMode(mode | IGMLoadSaveGame.Mode.Slot1);
                else if (Menu.IGMLoadSaveGame.GetMode().HasFlag(IGMLoadSaveGame.Mode.Slot2))
                    Menu.IGMLoadSaveGame.SetMode(mode | IGMLoadSaveGame.Mode.Slot2);
            }
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(IGMLoadSaveGame.Mode))
            {
                Save = e.HasFlag(IGMLoadSaveGame.Mode.Save);
                Slot = e.HasFlag(IGMLoadSaveGame.Mode.Slot);
                if (e.HasFlag(IGMLoadSaveGame.Mode.Checking))
                {
                    Show();
                    Refresh();
                }
                else
                    Hide();
            }
        }

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

        protected override void Init()
        {
            base.Init();

            ITEM[0, 0] = new IGMDataItem.Icon
            {
                Data = Icons.ID.Bar_BG,
                Pos = SIZE[0]
            };
            var r = SIZE[0];
            //r.Offset(0, 0);
            r.Inflate(-4, -4);
            TotalWidth = r.Width;

            LoadBarSlide = new Slide<float>(0, TotalWidth, Time, MathHelper.SmoothStep);
            r.Width = 0;
            ITEM[0, 1] = new IGMDataItem.Icon { Data = Icons.ID.Bar_Fill, Pos = r };
            Cursor_Status |= Cursor_Status.Enabled | Cursor_Status.Hidden | Cursor_Status.Static;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Height = 28;
            SIZE[i].Inflate(-10, 0);
            SIZE[i].Y += Height / 2 - SIZE[i].Height / 2;
        }

        #endregion Methods
    }
}