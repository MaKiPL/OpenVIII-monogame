using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DYING : JsmInstruction
    {
        public DYING()
        {
        }

        public DYING(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(DYING)}()";
        }
    }
}