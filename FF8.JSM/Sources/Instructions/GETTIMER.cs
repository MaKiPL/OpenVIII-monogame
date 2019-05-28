using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class GETTIMER : JsmInstruction
    {
        public GETTIMER()
        {
        }

        public GETTIMER(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(GETTIMER)}()";
        }
    }
}