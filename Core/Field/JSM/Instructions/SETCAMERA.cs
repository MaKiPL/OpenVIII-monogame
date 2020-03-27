namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SETCAMERA : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _arg0;

        #endregion Fields

        #region Constructors

        public SETCAMERA(IJsmExpression arg0) => _arg0 = arg0;

        public SETCAMERA(int parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SETCAMERA)}({nameof(_arg0)}: {_arg0})";

        #endregion Methods
    }
}