using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class DRAWPOINT : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DRAWPOINT(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DRAWPOINT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DRAWPOINT)}({nameof(_arg0)}: {_arg0})";
        }
    }
}