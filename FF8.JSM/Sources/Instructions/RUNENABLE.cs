using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class RUNENABLE : JsmInstruction
    {
        public RUNENABLE()
        {
        }

        public RUNENABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(RUNENABLE)}()";
        }
    }
}