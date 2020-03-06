namespace OpenVIII
{
    namespace Kernel
    {
        #region Classes

        /// <summary>
        /// Any ability a GF can learn.
        /// </summary>
        public abstract class Ability
        {
            ///// <summary>
            ///// Section Count
            ///// </summary>
            //public const int Count = 0;
            ///// <summary>
            ///// Section ID
            ///// </summary>
            //public const int BattleID = 0;

            #region Fields

            /// <summary>
            /// Default None Icon
            /// </summary>
            private const Icons.ID DefaultIcon = Icons.ID.None;

            /// <summary>
            /// Default Palette for Icons.
            /// </summary>
            private const byte DefaultPalette = 9;

            #endregion Fields

            #region Constructors

            protected Ability(FF8String name, FF8String description, byte ap, Icons.ID icon = DefaultIcon, byte palette = DefaultPalette)
                            => (Name, Description, AP, Icon, Palette) = (name, description, ap, icon, palette);

            #endregion Constructors

            #region Properties

            /// <summary>
            /// AP Required to learn ability
            /// </summary>
            public byte AP { get; }

            /// <summary>
            /// Description of ability.
            /// </summary>
            public FF8String Description { get; }

            /// <summary>
            /// Icon for this ability
            /// </summary>
            public Icons.ID Icon { get; }

            /// <summary>
            /// Name of ability
            /// </summary>
            public FF8String Name { get; }

            /// <summary>
            /// Palette for Icon
            /// </summary>
            public byte Palette { get; }

            #endregion Properties

            #region Methods

            public override string ToString() => Name;

            #endregion Methods
        }

        #endregion Classes
    }
}