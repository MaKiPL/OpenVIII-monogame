using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class FACEDIRSYNC : JsmInstruction
    {
        public FACEDIRSYNC()
        {
        }

        public FACEDIRSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FACEDIRSYNC)}()";
        }
    }
}