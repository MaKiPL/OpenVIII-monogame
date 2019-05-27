using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class Unknown8 : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public Unknown8(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public Unknown8(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown8)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}