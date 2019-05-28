using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class MOVEFLUSH : JsmInstruction
    {
        public MOVEFLUSH()
        {
        }

        public MOVEFLUSH(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVEFLUSH)}()";
        }
    }
}