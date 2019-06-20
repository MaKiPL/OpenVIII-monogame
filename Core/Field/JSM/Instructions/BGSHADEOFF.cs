using System;


namespace FF8
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