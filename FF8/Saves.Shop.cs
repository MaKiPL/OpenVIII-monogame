using System.IO;

namespace FF8
{
    public static partial class Saves
    {
        public struct Shop
        {
            #region Fields

            private byte[] items;
            private byte visited;

            /// <summary>
            /// padding ?
            /// </summary>
            private byte[] u1;

            #endregion Fields

            #region Methods

            public void Read(BinaryReader br)
            {//20
                items = br.ReadBytes(16);
                visited = br.ReadByte();
                u1 = br.ReadBytes(3);// padding ?
            }

            #endregion Methods
        }
    }
}