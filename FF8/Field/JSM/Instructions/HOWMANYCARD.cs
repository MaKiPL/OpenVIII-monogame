using System;


namespace FF8
{
    internal sealed class HOWMANYCARD : JsmInstruction
    {
        private IJsmExpression _arg0;

        public HOWMANYCARD(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public HOWMANYCARD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(HOWMANYCARD)}({nameof(_arg0)}: {_arg0})";
        }
    }
}