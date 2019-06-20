using System;


namespace FF8
{
    internal sealed class DOORLINEON : JsmInstruction
    {
        public DOORLINEON()
        {
        }

        public DOORLINEON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(DOORLINEON)}()";
        }
    }
}