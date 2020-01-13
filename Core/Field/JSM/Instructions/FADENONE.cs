using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADENONE : JsmInstruction
    {
        public FADENONE()
        {
        }

        public FADENONE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FADENONE)}()";
        }
    }
}