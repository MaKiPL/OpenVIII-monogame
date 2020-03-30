namespace OpenVIII.Fields.Scripts
{
    public sealed class StatelessServices : IServices
    {
        #region Constructors

        private StatelessServices()
        {
        }

        #endregion Constructors

        #region Properties

        public static IServices Instance { get; } = new StatelessServices();

        #endregion Properties

        #region Methods

        public T Service<T>(ServiceId<T> id) => (T)(object)id;

        #endregion Methods
    }
}