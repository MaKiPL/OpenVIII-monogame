using System.IO;

namespace FF8
{
    public partial class Kernel_bin
    {
        #region Classes
        /// <summary>
        /// Any ability a GF can learn.
        /// </summary>
        public abstract class Ability
        {
            #region Fields

            public const int count = 0;
            public const int id = 0;

            #endregion Fields 
            #region Properties

            public byte AP { get; protected set; }
            public FF8String Description { get; protected set; }
            public Icons.ID Icon { get; protected set; } = Icons.ID.None;
            public FF8String Name { get; protected set; }

            #endregion Properties

            #region Methods

            public abstract void Read(BinaryReader br, int i);

            public override string ToString() => Name;

            #endregion Methods
        }

        #endregion Classes
    }
}