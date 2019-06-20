using System;


namespace FF8
{
    internal sealed class SCROLLSYNC2 : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SCROLLSYNC2(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SCROLLSYNC2(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SCROLLSYNC2)}({nameof(_arg0)}: {_arg0})";
        }
    }
}