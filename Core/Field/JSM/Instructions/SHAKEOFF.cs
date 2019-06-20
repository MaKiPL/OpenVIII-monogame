using System;


namespace FF8
{
    internal sealed class SHAKEOFF : JsmInstruction
    {
        public SHAKEOFF()
        {
        }

        public SHAKEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SHAKEOFF)}()";
        }
    }
}