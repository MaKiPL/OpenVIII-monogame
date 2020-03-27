namespace OpenVIII.Fields
{
    public sealed class BreakAwaitable : IAwaitable
    {
        #region Properties

        public static IAwaitable Instance { get; } = new BreakAwaitable();

        #endregion Properties

        #region Methods

        public IAwaiter GetAwaiter() => DummyAwaiter.Instance;

        #endregion Methods
    }
}