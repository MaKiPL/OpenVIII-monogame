using System;


namespace OpenVIII
{
    internal sealed class SETDRAWPOINT : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SETDRAWPOINT(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SETDRAWPOINT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETDRAWPOINT)}({nameof(_arg0)}: {_arg0})";
        }
    }
}