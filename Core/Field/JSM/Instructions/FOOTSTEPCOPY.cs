using System;


namespace OpenVIII
{
    internal sealed class FOOTSTEPCOPY : JsmInstruction
    {
        public FOOTSTEPCOPY()
        {
        }

        public FOOTSTEPCOPY(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPCOPY)}()";
        }
    }
}