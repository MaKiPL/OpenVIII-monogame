using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class MAPJUMPON : JsmInstruction
    {
        public MAPJUMPON()
        {
        }

        public MAPJUMPON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MAPJUMPON)}()";
        }
    }
}