using System;


namespace FF8
{
    internal sealed class JOIN : JsmInstruction
    {
        public JOIN()
        {
        }

        public JOIN(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(JOIN)}()";
        }
    }
}