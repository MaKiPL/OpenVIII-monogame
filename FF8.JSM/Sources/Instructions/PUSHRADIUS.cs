using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class PUSHRADIUS : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PUSHRADIUS(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PUSHRADIUS(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PUSHRADIUS)}({nameof(_arg0)}: {_arg0})";
        }
    }
}