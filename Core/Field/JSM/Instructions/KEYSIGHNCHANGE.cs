namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Keysighnchange, only used on test
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/150_KEYSIGHNCHANGE&action=edit&redlink=1"/>
    public sealed class KEYSIGHNCHANGE : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public KEYSIGHNCHANGE(IJsmExpression arg0) => _arg0 = arg0;

        public KEYSIGHNCHANGE(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(KEYSIGHNCHANGE)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}