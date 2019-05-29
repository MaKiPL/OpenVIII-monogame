using System;


namespace FF8
{
    internal sealed class POPANIME : JsmInstruction
    {
        public POPANIME()
        {
        }

        public POPANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(POPANIME)}()";
        }
    }
}