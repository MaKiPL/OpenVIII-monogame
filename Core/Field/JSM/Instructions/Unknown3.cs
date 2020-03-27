namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Unknown3
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/168_UNKNOWN3&action=edit&redlink=1"/>
    public sealed class Unknown3 : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public Unknown3(IJsmExpression arg0) => _arg0 = arg0;

        public Unknown3(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown3)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}