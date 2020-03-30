namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Used only twice, on the Ragnarok hatch screen.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/169_UNKNOWN4"/>
    public sealed class Unknown4 : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// 1
        /// </summary>
        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public Unknown4(IJsmExpression arg0) => _arg0 = arg0;

        public Unknown4(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(Unknown4)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}