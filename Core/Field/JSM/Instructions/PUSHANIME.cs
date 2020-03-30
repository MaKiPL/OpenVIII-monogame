namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Push animation on stack?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/14A_PUSHANIME&action=edit&redlink=1"/>
    public sealed class PUSHANIME : JsmInstruction
    {
        #region Constructors

        public PUSHANIME()
        {
        }

        public PUSHANIME(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(PUSHANIME)}()";

        #endregion Methods
    }
}