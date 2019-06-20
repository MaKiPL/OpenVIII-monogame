using System;


namespace OpenVIII
{
    internal sealed class LINEOFF : JsmInstruction
    {
        public LINEOFF()
        {
        }

        public LINEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(LINEOFF)}()";
        }
    }
}