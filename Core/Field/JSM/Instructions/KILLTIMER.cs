using System;


namespace OpenVIII
{
    internal sealed class KILLTIMER : JsmInstruction
    {
        public KILLTIMER()
        {
        }

        public KILLTIMER(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(KILLTIMER)}()";
        }
    }
}