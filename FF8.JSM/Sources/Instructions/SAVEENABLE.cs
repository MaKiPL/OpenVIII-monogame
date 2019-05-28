using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class SAVEENABLE : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SAVEENABLE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SAVEENABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SAVEENABLE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}