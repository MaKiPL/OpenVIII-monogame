using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public class DeadTime : Slide<int>
    {
        private static int Lerp(int start, int end, float amount) => checked((int)MathHelper.Lerp(start, end, amount));

        private static TimeSpan totalTime(int start) => TimeSpan.FromMilliseconds((1000d / 15d) * start);

        public DeadTime() : base(200, 0, totalTime(200), Lerp) => Repeat = true;

        protected override void CheckRepeat()
        {
            if (Done)
            {
                DoneEvent?.Invoke(this, End);
            }
            base.CheckRepeat();
        }

        public override void GotoEnd()
        {
            DoneEvent?.Invoke(this, End);
            base.GotoEnd();
        }

        public event EventHandler<int> DoneEvent;
    }
}