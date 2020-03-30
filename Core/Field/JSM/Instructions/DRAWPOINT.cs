namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DRAWPOINT : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public DRAWPOINT(IJsmExpression arg0) => _arg0 = arg0;

        public DRAWPOINT(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(DRAWPOINT)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}