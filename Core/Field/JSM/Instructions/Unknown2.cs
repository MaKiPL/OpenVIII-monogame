namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Unknown2
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/167_UNKNOWN2&action=edit&redlink=1"/>
    public sealed class Unknown2 : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public Unknown2(IJsmExpression arg0) => _arg0 = arg0;

        public Unknown2(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown2)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}