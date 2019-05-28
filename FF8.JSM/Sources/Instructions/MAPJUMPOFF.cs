using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class MAPJUMPOFF : JsmInstruction
    {
        public MAPJUMPOFF()
        {
        }

        public MAPJUMPOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MAPJUMPOFF)}()";
        }
    }
}