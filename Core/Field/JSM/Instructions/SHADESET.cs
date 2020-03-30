namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Shade set?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0B3_SHADESET&action=edit&redlink=1"/>
    public sealed class SHADESET : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public SHADESET(IJsmExpression arg0) => _arg0 = arg0;

        public SHADESET(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SHADESET)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}