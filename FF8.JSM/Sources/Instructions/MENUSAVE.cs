using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class MENUSAVE : JsmInstruction
    {
        public MENUSAVE()
        {
        }

        public MENUSAVE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUSAVE)}()";
        }
    }
}