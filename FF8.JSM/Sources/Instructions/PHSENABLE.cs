using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class PHSENABLE : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PHSENABLE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PHSENABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PHSENABLE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}