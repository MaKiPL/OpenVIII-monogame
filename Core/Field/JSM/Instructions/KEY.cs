using System;


namespace FF8
{
    internal sealed class KEY : JsmInstruction
    {
        private IJsmExpression _arg0;

        public KEY(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public KEY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEY)}({nameof(_arg0)}: {_arg0})";
        }
    }
}