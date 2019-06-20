using System;


namespace OpenVIII
{
    internal sealed class LASTOUT : JsmInstruction
    {
        public LASTOUT()
        {
        }

        public LASTOUT(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(LASTOUT)}()";
        }
    }
}