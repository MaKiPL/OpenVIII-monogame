namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Swap?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/162_SWAP&action=edit&redlink=1"/>
    public sealed class SWAP : JsmInstruction
    {
        #region Constructors

        public SWAP()
        {
        }

        public SWAP(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SWAP)}()";

        #endregion Methods
    }
}