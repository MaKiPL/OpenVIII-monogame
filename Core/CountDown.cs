using System;

namespace OpenVIII
{
    /// <summary>
    /// Countdown from a set value based on game elapsed time
    /// </summary>
    /// <see cref="https://www.dreamincode.net/forums/topic/175513-countdown-timer/"/>
    public class CountDown
    {
        #region Constructors

        public CountDown(TimeSpan timeSpan) => TS = timeSpan;

        public CountDown(double ms) => MS = ms;

        #endregion Constructors

        #region Properties

        public bool Done => TS <= TimeSpan.Zero;

        public double MS
        {
            get => TS.TotalMilliseconds;
            set => TS = TimeSpan.FromMilliseconds(value);
        }

        public TimeSpan TS { get; set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Run Update() to pass time; To "pause" don't run update
        /// </summary>
        public bool Update()
        {
            if (!Done)
            {
                TS -= Memory.ElapsedGameTime;
            }
            return Done;
        }

        #endregion Methods
    }
}