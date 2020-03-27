namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes to blink? Unused!
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_BLINKEYES&action=edit&redlink=1"/>
    public sealed class BLINKEYES : JsmInstruction
    {
        #region Constructors

        public BLINKEYES()
        {
        }

        public BLINKEYES(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(BLINKEYES)}()";

        #endregion Methods
    }
}