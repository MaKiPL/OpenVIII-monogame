using Microsoft.Xna.Framework;
using System;

namespace OpenVIII.IGMData.Limit
{
    public class Shot : IGMData.Base
    {
        public float CriticalPercent { get; private set; }
        private bool skipupdate;
        private Slide<float> timeSlide;

        protected override void Init()
        {
            timeSlide = new Slide<float>(100f, 0f, TimeSpan.Zero, MathHelper.Lerp);
            base.Init();
        }

        public IGMDataItem.Icon BULLET { get => (IGMDataItem.Icon)ITEM[0, 0]; set => ITEM[0, 0] = value; }
        public IGMDataItem.Integer QTY { get => (IGMDataItem.Integer)ITEM[1, 0]; set => ITEM[1, 0] = value; }
        public IGMDataItem.Icon TIME { get => (IGMDataItem.Icon)ITEM[2, 0]; set => ITEM[2, 0] = value; }
        public IGMDataItem.Icon BRACKET { get => (IGMDataItem.Icon)ITEM[3, 0]; set => ITEM[3, 0] = value; }
        public IGMDataItem.Gradient.GF BLUEBAR { get => (IGMDataItem.Gradient.GF) ITEM[4, 0]; set => ITEM[4, 0] = value; }

    public override void Refresh()
        {
            if (Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
            {
                TimeSpan timeleft;
                if (c.CurrentCrisisLevel >= 0)
                    switch (c.CurrentCrisisLevel)
                    {
                        case 0:
                            timeleft = TimeSpan.FromSeconds(8);
                            break;

                        case 1:
                            timeleft = TimeSpan.FromSeconds(12);
                            break;

                        case 2:
                            timeleft = TimeSpan.FromSeconds(20);
                            break;

                        case 3:
                        default:
                            timeleft = TimeSpan.FromSeconds(26.6);
                            break;
                    }
                else
                    timeleft = TimeSpan.Zero;
                timeSlide.TotalTime = timeleft;
                timeSlide.Restart();
                CriticalPercent = (c.LUCK + 26) / 256f;
                skipupdate = true;
            }
            base.Refresh();
        }

        public override bool Update()
        {
            if (skipupdate)
            {
                timeSlide.Update();
            }
            skipupdate = false;
            return base.Update();
        }
    }
}