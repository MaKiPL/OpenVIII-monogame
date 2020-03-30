namespace OpenVIII.Fields
{
    public static partial class Field
    {
        #region Classes

        public sealed class Info
        {
            #region Fields

            private readonly IDataProvider _dataProvider;

            #endregion Fields

            #region Constructors

            public Info(string fieldName, IDataProvider dataProvider)
            {
                Name = fieldName;
                _dataProvider = dataProvider;
            }

            #endregion Constructors

            #region Properties

            public string Name { get; }

            #endregion Properties

            #region Methods

            public bool TryReadData(Part part, out byte[] data)
            {
                data = _dataProvider.FindPart(part);
                return data != null;
            }

            #endregion Methods
        }

        #endregion Classes
    }
}