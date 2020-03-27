namespace OpenVIII.Fields
{
    public interface IAwaitable
    {
        #region Methods

        IAwaiter GetAwaiter();

        #endregion Methods
    }
}