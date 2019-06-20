using System;

namespace OpenVIII
{
    public static partial class Field
    {
        public sealed class Info
        {
            public String Name { get; }

            private readonly IDataProvider _dataProvider;

            public Info(String fieldName, IDataProvider dataProvider)
            {
                Name = fieldName;
                _dataProvider = dataProvider;
            }

            public Boolean TryReadData(Part part, out Byte[] data)
            {
                data = _dataProvider.FindPart(part);
                return data != null;
            }
        }
    }
}