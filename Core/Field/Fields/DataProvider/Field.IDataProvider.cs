using System;

namespace OpenVIII
{
    public static partial class Field
    {
        public interface IDataProvider
        {
            Byte[] FindPart(Part part);
        }
    }
}