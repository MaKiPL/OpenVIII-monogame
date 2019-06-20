using System;


namespace FF8
{
    internal sealed class BGCLEAR : JsmInstruction
    {
        private IJsmExpression _arg0;

        public BGCLEAR(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public BGCLEAR(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGCLEAR)}({nameof(_arg0)}: {_arg0})";
        }
    }
}