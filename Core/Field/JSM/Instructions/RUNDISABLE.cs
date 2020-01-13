using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class RUNDISABLE : JsmInstruction
    {
        public RUNDISABLE()
        {
        }

        public RUNDISABLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(RUNDISABLE)}()";
        }
    }
}