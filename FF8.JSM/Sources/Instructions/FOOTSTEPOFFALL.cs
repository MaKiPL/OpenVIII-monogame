using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FOOTSTEPOFFALL : JsmInstruction
    {
        public FOOTSTEPOFFALL()
        {
        }

        public FOOTSTEPOFFALL(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPOFFALL)}()";
        }
    }
}