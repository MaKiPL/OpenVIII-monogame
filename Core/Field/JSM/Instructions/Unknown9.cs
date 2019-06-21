using System;


namespace OpenVIII
{
    internal sealed class Unknown9 : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public Unknown9(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public Unknown9(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown9)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}