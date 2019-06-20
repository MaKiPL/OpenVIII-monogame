using System;


namespace FF8
{
    internal sealed class MENUPHS : JsmInstruction
    {
        public MENUPHS()
        {
        }

        public MENUPHS(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUPHS)}()";
        }
    }
}