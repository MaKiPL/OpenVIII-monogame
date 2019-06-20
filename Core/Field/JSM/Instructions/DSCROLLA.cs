using System;


namespace FF8
{
    internal sealed class DSCROLLA : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DSCROLLA(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DSCROLLA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DSCROLLA)}({nameof(_arg0)}: {_arg0})";
        }
    }
}