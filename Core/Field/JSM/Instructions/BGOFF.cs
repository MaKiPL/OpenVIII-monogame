using System;


namespace OpenVIII.Fields
{
    internal sealed class BGOFF : JsmInstruction
    {
        public BGOFF()
        {
        }

        public BGOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGOFF)}()";
        }
    }
}