using System;

namespace OpenVIII
{
    public class ATBTimer
    {
        private bool _first = true;

        public int ATBBarIncrement { get; private set; }
        public float ATBBarPos { get; private set; }
        public bool Done => Percent >= 1f;
        public float Percent
        {
            get
            {
                var percent = Math.Abs(ATBBarPos / _damageable.ATBBarSize);
                return percent > 1f ? 1f : percent;
            }
        }

        private Damageable _damageable;

        public ATBTimer(Damageable damageable) => Refresh(damageable);

        /// <summary>
        /// Refresh damageable and start new turn. if Damageable is changed.
        /// </summary>
        /// <param name="damageable">Character,GF,Enemy</param>
        public void Refresh(Damageable damageable)
        {
            if (damageable == _damageable) return;
            _damageable = damageable;
            FirstTurn();
        }

        /// <summary>
        /// Start new turn.
        /// </summary>
        public void NewTurn()
        {
            if (_first)
            {
                ATBBarPos = _damageable?.ATBBarStart() ?? 0;
                _first = false;
            }
            else
                ATBBarPos = 0;
        }

        /// <summary>
        /// Start over.
        /// </summary>
        public void FirstTurn()
        {
            _first = true;
            _damageable?.Charging();
            NewTurn();
        }

        /// <summary>
        /// Reset to defaultState
        /// </summary>
        public void Reset() => FirstTurn();

        public bool Update()
        {
            if (_damageable == null || Done)
            {
                return false;
            }

            if (_damageable.IsDead && ATBBarPos > 0)
            {
                ATBBarPos = 0;
                return true;
            }

            var totalMilliseconds = Memory.ElapsedGameTime.TotalMilliseconds;
            ATBBarIncrement = _damageable.BarIncrement(); // 60 ticks per second.
            ATBBarPos += (float)(ATBBarIncrement * totalMilliseconds / 60);
            // if TotalMilliseconds is 1000 then it'll increment 60 times. So this should be right.
            return true;
        }
    }
}
