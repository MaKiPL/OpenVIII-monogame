namespace OpenVIII.Fields
{
    public sealed class SpinAwaitable : IAwaitable
    {
        #region Properties

        public static IAwaitable Instance { get; } = new SpinAwaitable();

        #endregion Properties

        #region Methods

        public IAwaiter GetAwaiter() => DummyAwaiter.Instance;

        #endregion Methods
    }
}