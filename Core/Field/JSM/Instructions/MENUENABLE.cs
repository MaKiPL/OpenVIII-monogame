using System;


namespace OpenVIII
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