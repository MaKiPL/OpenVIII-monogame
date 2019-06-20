using System;


namespace FF8
{
    internal sealed class Unknown5 : JsmInstruction
    {
        private IJsmExpression _arg0;

        public Unknown5(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown5(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown5)}({nameof(_arg0)}: {_arg0})";
        }
    }
}