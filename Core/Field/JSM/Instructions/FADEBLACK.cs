using System;


namespace FF8
{
    internal sealed class FADEBLACK : JsmInstruction
    {
        public FADEBLACK()
        {
        }

        public FADEBLACK(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FADEBLACK)}()";
        }
    }
}