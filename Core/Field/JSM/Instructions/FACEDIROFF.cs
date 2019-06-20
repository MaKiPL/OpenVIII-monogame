using System;


namespace FF8
{
    internal sealed class FACEDIROFF : JsmInstruction
    {
        private IJsmExpression _arg0;

        public FACEDIROFF(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public FACEDIROFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(FACEDIROFF)}({nameof(_arg0)}: {_arg0})";
        }
    }
}