namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Background Animation Sync
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/098_BGANIMESYNC&action=edit&redlink=1"/>
    public sealed class BGANIMESYNC : JsmInstruction
    {
        #region Constructors

        public BGANIMESYNC()
        {
        }

        public BGANIMESYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BGANIMESYNC)}()";

        #endregion Methods
    }
}