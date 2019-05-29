using System;


namespace FF8
{
    internal sealed class FOLLOWOFF : JsmInstruction
    {
        public FOLLOWOFF()
        {
        }

        public FOLLOWOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOLLOWOFF)}()";
        }
    }
}