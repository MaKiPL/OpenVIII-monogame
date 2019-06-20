using System;


namespace OpenVIII
{
    internal sealed class DISC : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DISC(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DISC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DISC)}({nameof(_arg0)}: {_arg0})";
        }
    }
}