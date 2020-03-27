namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes open? Unused!
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_OPENEYES&action=edit&redlink=1"/>
    public sealed class OPENEYES : JsmInstruction
    {
        #region Constructors

        public OPENEYES()
        {
        }

        public OPENEYES(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(OPENEYES)}()";

        #endregion Methods
    }
}