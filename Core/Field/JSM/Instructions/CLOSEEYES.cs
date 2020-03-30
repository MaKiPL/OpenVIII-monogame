namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes closed
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_CLOSEEYES&action=edit&redlink=1"/>
    public sealed class CLOSEEYES : JsmInstruction
    {
        #region Constructors

        public CLOSEEYES()
        {
        }

        public CLOSEEYES(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(CLOSEEYES)}()";

        #endregion Methods
    }
}