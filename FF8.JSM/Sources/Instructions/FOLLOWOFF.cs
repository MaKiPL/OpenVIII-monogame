using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FOLLOWOFF : JsmInstruction
    {
        public FOLLOWOFF()
        {
        }

        public FOLLOWOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOLLOWOFF)}()";
        }
    }
}