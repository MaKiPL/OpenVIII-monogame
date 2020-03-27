namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SCROLLMODE2 : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;
        private readonly IJsmExpression _arg1;
        private readonly IJsmExpression _arg2;
        private readonly IJsmExpression _arg3;
        private readonly IJsmExpression _arg4;

        #endregion Fields

        #region Constructors

        public SCROLLMODE2(IJsmExpression arg0, IJsmExpression arg1, IJsmExpression arg2, IJsmExpression arg3, IJsmExpression arg4)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
            _arg4 = arg4;
        }

        public SCROLLMODE2(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg4: stack.Pop(),
                arg3: stack.Pop(),
                arg2: stack.Pop(),
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SCROLLMODE2)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1}, {nameof(_arg2)}: {_arg2}, {nameof(_arg3)}: {_arg3}, {nameof(_arg4)}: {_arg4})";

        #endregion Methods
    }
}