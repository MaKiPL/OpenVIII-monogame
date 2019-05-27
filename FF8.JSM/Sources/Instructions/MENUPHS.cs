using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class MENUPHS : JsmInstruction
    {
        public MENUPHS()
        {
        }

        public MENUPHS(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUPHS)}()";
        }
    }
}