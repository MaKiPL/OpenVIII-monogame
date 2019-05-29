using System;


namespace FF8
{
    internal sealed class MENUENABLE : JsmInstruction
    {
        public MENUENABLE()
        {
        }

        public MENUENABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUENABLE)}()";
        }
    }
}