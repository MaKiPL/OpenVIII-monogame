using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class BGOFF : JsmInstruction
    {
        public BGOFF()
        {
        }

        public BGOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGOFF)}()";
        }
    }
}