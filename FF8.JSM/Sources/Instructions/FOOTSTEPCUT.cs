using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FOOTSTEPCUT : JsmInstruction
    {
        public FOOTSTEPCUT()
        {
        }

        public FOOTSTEPCUT(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPCUT)}()";
        }
    }
}