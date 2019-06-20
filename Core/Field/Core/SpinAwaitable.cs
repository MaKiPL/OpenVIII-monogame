namespace FF8
{
    public sealed class SpinAwaitable : IAwaitable
    {
        public static IAwaitable Instance { get; } = new SpinAwaitable();

        public IAwaiter GetAwaiter()
        {
            return DummyAwaiter.Instance;
        }
    }
}