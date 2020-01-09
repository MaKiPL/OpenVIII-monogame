using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class JOIN : JsmInstruction
    {
        public JOIN()
        {
        }

        public JOIN(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(JOIN)}()";
        }
    }
}