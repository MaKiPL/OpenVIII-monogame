using System;

namespace FF8.Fields
{
    public static partial class Field
    {
        public interface IDataProvider
        {
            Byte[] FindPart(Part part);
        }
    }
}