using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class INITTRACE : JsmInstruction
    {
        public INITTRACE()
        {
        }

        public INITTRACE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(INITTRACE)}()";
        }
    }
}