namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Performs no operation.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/000_NOP"/>
    internal sealed class NOP : JsmInstruction
    {
        #region Constructors

        public NOP()
        {
        }

        public NOP(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(NOP)}()";

        #endregion Methods
    }
}