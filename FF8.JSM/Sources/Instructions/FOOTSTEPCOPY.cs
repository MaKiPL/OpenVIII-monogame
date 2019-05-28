using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FOOTSTEPCOPY : JsmInstruction
    {
        public FOOTSTEPCOPY()
        {
        }

        public FOOTSTEPCOPY(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPCOPY)}()";
        }
    }
}