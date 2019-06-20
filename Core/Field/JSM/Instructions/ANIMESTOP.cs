using System;


namespace FF8
{
    internal sealed class ANIMESTOP : JsmInstruction
    {
        public ANIMESTOP()
        {
        }

        public ANIMESTOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(ANIMESTOP)}()";
        }
    }
}