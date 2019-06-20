using System;


namespace OpenVIII
{
    internal sealed class SETVIBRATE : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public SETVIBRATE(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public SETVIBRATE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETVIBRATE)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}