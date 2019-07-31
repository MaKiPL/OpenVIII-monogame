using System.Collections.Generic;

namespace OpenVIII
{
    public abstract class Input2
    {
        #region Fields

        protected static bool bLimitInput;
        protected static List<Input2> Data;
        protected static double msDelay;
        private static readonly int msDelayLimit = 100;

        #endregion Fields

        #region Constructors

        public Input2()
        {
            if (Data != null)
                Data = new List<Input2>
                {
                    new InputKeyboard(),
                    new InputMouse(),
                    new InputGamePad(),
                };
            if (InputList != null)
                InputList = new List<Inputs>
                {
                    new Inputs_OpenVIII(),
                    new Inputs_FF8PSX(),
                    new Inputs_FF8Steam(),
                    new Inputs_FF82000()
                };
        }

        #endregion Constructors

        #region Methods

        public static bool Update()
        {
            CheckInputLimit();
            Data.ForEach(a => a.UpdateOnce());
            return false;
        }

        protected abstract bool UpdateOnce();

        private static void CheckInputLimit()
        {
            //issue here if CheckInputLimit is checked more than once per update cycle this will be wrong.
            if (Memory.gameTime != null)
                bLimitInput = (msDelay += Memory.gameTime.ElapsedGameTime.TotalMilliseconds) < msDelayLimit;
            if (!bLimitInput) msDelay = 0;
        }

        public static List<Inputs> InputList { get; private set; }

        #endregion Methods
    }
}