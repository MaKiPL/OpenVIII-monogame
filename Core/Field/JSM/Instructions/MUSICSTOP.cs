namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MUSICSTOP : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public MUSICSTOP(IJsmExpression arg0) => _arg0 = arg0;

        public MUSICSTOP(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(MUSICSTOP)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}