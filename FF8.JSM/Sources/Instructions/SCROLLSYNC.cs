using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class SCROLLSYNC : JsmInstruction
    {
        public SCROLLSYNC()
        {
        }

        public SCROLLSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SCROLLSYNC)}()";
        }
    }
}