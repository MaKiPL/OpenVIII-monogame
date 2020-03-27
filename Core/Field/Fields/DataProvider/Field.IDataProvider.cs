namespace OpenVIII.Fields
{
    public static partial class Field
    {
        #region Interfaces

        public interface IDataProvider
        {
            #region Methods

            byte[] FindPart(Part part);

            #endregion Methods
        }

        #endregion Interfaces
    }
}