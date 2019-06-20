using System;


namespace FF8
{
    internal sealed class ISPARTY : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ISPARTY(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ISPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ISPARTY)}({nameof(_arg0)}: {_arg0})";
        }
    }
}