namespace OpenVIII.Fields
{
    public sealed class DummyAwaitable : IAwaitable
    {
        #region Properties

        public static IAwaitable Instance { get; } = new DummyAwaitable();

        #endregion Properties

        #region Methods

        public IAwaiter GetAwaiter() => DummyAwaiter.Instance;

        #endregion Methods
    }
}