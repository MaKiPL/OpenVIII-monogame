using System;


namespace FF8
{
    internal sealed class Unknown12 : JsmInstruction
    {
        public Unknown12()
        {
        }

        public Unknown12(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown12)}()";
        }
    }
}