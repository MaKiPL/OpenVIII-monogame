namespace OpenVIII.Kernel
{
    /// <summary>
    /// Any ability a GF can learn.
    /// </summary>
    public interface IAbility
    {
        #region Properties

        /// <summary>
        /// AP Required to learn ability
        /// </summary>
        byte AP { get; }

        /// <summary>
        /// Description of ability.
        /// </summary>
        FF8String Description { get; }

        /// <summary>
        /// Icon for this ability
        /// </summary>
        Icons.ID Icon { get; }

        /// <summary>
        /// Name of ability
        /// </summary>
        FF8String Name { get; }

        /// <summary>
        /// Palette for Icon
        /// </summary>
        byte Palette { get; }

        #endregion Properties
    }
}