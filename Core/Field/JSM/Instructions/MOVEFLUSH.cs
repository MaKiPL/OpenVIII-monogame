namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Flush Movement; Not confirmed, but I'm pretty sure it halts the current entity's movements.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/148_MOVEFLUSH"/>
    public sealed class MOVEFLUSH : JsmInstruction
    {
        #region Constructors

        public MOVEFLUSH()
        {
        }

        public MOVEFLUSH(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MOVEFLUSH)}()";

        #endregion Methods
    }
}