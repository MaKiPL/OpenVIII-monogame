using System.IO;

namespace FF8
{
    public static partial class Saves
    {
        #region Classes

        public class Shop
        {

            #region Fields

            private byte[] items;
            /// <summary>
            /// padding ?
            /// </summary>
            private byte[] u1;

            private byte visited;

            #endregion Fields

            #region Constructors

            public Shop()
            {
            }

            public Shop(BinaryReader br) => Read(br);

            #endregion Constructors

            #region Methods

            public void Read(BinaryReader br)
            {//20
                items = br.ReadBytes(16);
                visited = br.ReadByte();
                u1 = br.ReadBytes(3);// padding ?
            }
            //shadow clone
            public Shop Clone() => (Shop)MemberwiseClone();

            #endregion Methods

        }

        #endregion Classes
    }
}