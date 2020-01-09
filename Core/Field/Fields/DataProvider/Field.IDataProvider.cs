using System;

namespace OpenVIII.Fields
{
    public static partial class Field
    {
        public interface IDataProvider
        {
            Byte[] FindPart(Part part);
        }
    }
}