using System;


namespace FF8
{
    internal sealed class MOVECANCEL : JsmInstruction
    {
        private IJsmExpression _arg0;

        public MOVECANCEL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public MOVECANCEL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVECANCEL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}