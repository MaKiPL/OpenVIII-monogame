using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class DYING : JsmInstruction
    {
        public DYING()
        {
        }

        public DYING(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(DYING)}()";
        }
    }
}