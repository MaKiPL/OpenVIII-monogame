using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class BGSHADEOFF : JsmInstruction
    {
        public BGSHADEOFF()
        {
        }

        public BGSHADEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGSHADEOFF)}()";
        }
    }
}