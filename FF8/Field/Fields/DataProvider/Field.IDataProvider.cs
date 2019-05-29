using System;

namespace FF8
{
    public static partial class Field
    {
        public interface IDataProvider
        {
            Byte[] FindPart(Part part);
        }
    }
}