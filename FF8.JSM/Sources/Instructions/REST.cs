using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class REST : JsmInstruction
    {
        public REST()
        {
        }

        public REST(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(REST)}()";
        }
    }
}