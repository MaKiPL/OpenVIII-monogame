using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class DIRA : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DIRA(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DIRA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DIRA)}({nameof(_arg0)}: {_arg0})";
        }
    }
}