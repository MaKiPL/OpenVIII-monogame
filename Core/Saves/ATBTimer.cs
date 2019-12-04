using System;

namespace OpenVIII
{
    public class ATBTimer
    {
        private bool First = true;

        public int ATBBarIncrement { get; private set; } = 0;
        public float ATBBarPos { get; private set; } = 0;
        public bool Done => Percent >= 1f;
        public float Percent
        {
            get
            {
                float percent = Math.Abs(ATBBarPos / Damageable.ATBBarSize);
                return percent > 1f ? 1f : percent;
            }
        }

        private Damageable Damageable;

        public ATBTimer(Damageable damageable) => Damageable = damageable;

        /// <summary>
        /// Refresh damageable and start new turn.
        /// </summary>
        /// <param name="damageable">Character,GF,Enemy</param>
        public void Refresh(Damageable damageable)
        {
            Damageable = damageable;
            NewTurn();
        }

        /// <summary>
        /// Start new turn.
        /// </summary>
        public void NewTurn()
        {
            if (First)
            {
                ATBBarPos = Damageable?.ATBBarStart() ?? 0;
                First = false;
            }
            else

                ATBBarPos = 0;
        }

        /// <summary>
        /// Start over.
        /// </summary>
        public void FirstTurn()
        {
            First = true;
            NewTurn();
        }

        /// <summary>
        /// Reset to defaultState
        /// </summary>
        public void Reset() => FirstTurn();

        public bool Update()
        {
            if (Damageable != null)
            {
                if (!Damageable.IsDead)
                {
                    if (!Done)
                    {
                        double TotalMilliseconds = Memory.gameTime.ElapsedGameTime.TotalMilliseconds;
                        ATBBarIncrement = Damageable.BarIncrement(); // 60 ticks per second.
                        ATBBarPos += checked((float)(ATBBarIncrement * TotalMilliseconds / 60));
                        // if TotalMilliseconds is 1000 then it'll increment 60 times. So this should be right.
                        return true;
                    }
                }
                else if (ATBBarPos > 0)
                {
                    ATBBarPos = 0;
                    return true;
                }
            }
            return false;
        }
    }
}