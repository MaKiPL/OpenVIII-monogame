using System;


namespace FF8
{
    internal sealed class SCROLLSYNC : JsmInstruction
    {
        public SCROLLSYNC()
        {
        }

        public SCROLLSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SCROLLSYNC)}()";
        }
    }
}