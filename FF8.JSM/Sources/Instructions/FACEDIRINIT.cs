using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FACEDIRINIT : JsmInstruction
    {
        public FACEDIRINIT()
        {
        }

        public FACEDIRINIT(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FACEDIRINIT)}()";
        }
    }
}