using System;


namespace FF8
{
    internal sealed class WHOAMI : JsmInstruction
    {
        private IJsmExpression _arg0;

        public WHOAMI(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public WHOAMI(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(WHOAMI)}({nameof(_arg0)}: {_arg0})";
        }
    }
}