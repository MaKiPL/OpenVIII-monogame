using System;

namespace OpenVIII.Fields
{
    public sealed class DummyAwaiter : IAwaiter
    {
        #region Properties

        public static IAwaiter Instance { get; } = new DummyAwaiter();

        public bool IsCompleted => true;

        #endregion Properties

        #region Methods

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation) => continuation();

        #endregion Methods
    }
}