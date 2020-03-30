using System;
using System.Linq;
using Microsoft.Xna.Framework;
using OpenVIII.IGMDataItem;
using OpenVIII.IGMDataItem.Gradient;

namespace OpenVIII.IGMData.Limit
{
    public class Shot : Base
    {
        #region Fields

        private ItemInMenu ammo;
        private bool skipupdate;
        public Damageable[] Targets { get; set; }
        private Slide<float> timeSlide;

        #endregion Fields

        #region Properties
        private GF BLUEBAR { get => (GF)ITEM[3, 0]; set => ITEM[3, 0] = value; }
        private Icon BRACKET { get => (Icon)ITEM[4, 0]; set => ITEM[4, 0] = value; }
        private Icon BULLET { get => (Icon)ITEM[0, 0]; set => ITEM[0, 0] = value; }
        public float CriticalPercent { get; set; }
        private Integer QTY { get => (Integer)ITEM[1, 0]; set => ITEM[1, 0] = value; }

        public Icon TIME { get => (Icon)ITEM[2, 0]; set => ITEM[2, 0] = value; }

        #endregion Properties

        #region Methods

        public static Shot Create(Rectangle pos, Damageable damageable = null) => Create<Shot>(5, 1, new Empty { Pos = pos }, 1, 4, damageable, battle: true);

        public override bool Inputs() => base.Inputs() || true;

        public override bool Inputs_OKAY()
        {
            skipsnd = true;
            Memory.State.EarnItem(ammo.ID, -1);
            return base.Inputs_OKAY();
        }

        public override bool Inputs_RotateRight() => Inputs_OKAY();

        public void Refresh(ItemInMenu ammo, Damageable[] d)
        {
            this.ammo = ammo;
            Targets = d;
            Refresh();
        }

        public override void Refresh()
        {
            if (Damageable != null && Damageable.GetCharacterData(out var c))
            {
                TimeSpan timeleft;
                switch (c.CurrentCrisisLevel)
                {
                    case 0:
                    default:
                        timeleft = TimeSpan.FromSeconds(8);
                        break;

                    case 1:
                        timeleft = TimeSpan.FromSeconds(12);
                        break;

                    case 2:
                        timeleft = TimeSpan.FromSeconds(20);
                        break;

                    case 3:
                        timeleft = TimeSpan.FromSeconds(26.6);
                        break;
                }
                timeSlide.TotalTime = timeleft;
                timeSlide.Restart();
                CriticalPercent = (c.LUCK + 26) / 256f;
                skipupdate = true;
            }
            base.Refresh();
        }

        public override bool Update()
        {
            base.Update();
            if (Enabled)
            {
                QTY.Data = Memory.State.Items.FirstOrDefault(x => x.ID == ammo.ID).QTY;
                if (!skipupdate)
                {
                    timeSlide.Update();
                    BLUEBAR.Update(timeSlide.CurrentPercent);
                }
                if (timeSlide.Done || QTY.Data == 0)
                {
                    Hide();
                    Damageable?.EndTurn();
                }
                skipupdate = false;
                return true;
            }
            return false;
        }

        protected override void Init()
        {
            timeSlide = new Slide<float>(100f, 0f, TimeSpan.Zero, MathHelper.Lerp);
            base.Init();
            Memory.Icons.Trim(Icons.ID.BULLET, 2);
            Memory.Icons.Trim(Icons.ID.TIME, 2);
            BULLET = new Icon { Data = Icons.ID.BULLET, Pos = SIZE[0] };
            const int QTYwidth = 60;
            QTY = new Integer { Data = 0, Pos = new Rectangle(SIZE[1].Right - QTYwidth, SIZE[1].Top, QTYwidth, SIZE[1].Height), Spaces = 3 };
            TIME = new Icon { Data = Icons.ID.TIME, Pos = SIZE[2] };
            SIZE[3].Height = 15;
            BLUEBAR = GF.Create(SIZE[3]);
            BRACKET = new Icon { Data = Icons.ID.Size_08x64_Bar, Pos = SIZE[3] };

            Cursor_Status = Cursor_Status.Enabled | Cursor_Status.Static | Cursor_Status.Hidden;

            Hide();
        }
        public override bool Inputs_CANCEL()
        {
            skipsnd = true;
            return base.Inputs_CANCEL() || true;
        }

        #endregion Methods
    }
}