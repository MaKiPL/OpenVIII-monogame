using System;


namespace OpenVIII
{
    internal sealed class POPANIME : JsmInstruction
    {
        public POPANIME()
        {
        }

        public POPANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(POPANIME)}()";
        }
    }
}