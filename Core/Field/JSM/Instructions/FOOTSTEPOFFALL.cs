using System;


namespace OpenVIII
{
    internal sealed class FOOTSTEPOFFALL : JsmInstruction
    {
        public FOOTSTEPOFFALL()
        {
        }

        public FOOTSTEPOFFALL(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPOFFALL)}()";
        }
    }
}