using System;


namespace OpenVIII
{
    internal sealed class Unknown10 : JsmInstruction
    {
        public Unknown10()
        {
        }

        public Unknown10(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown10)}()";
        }
    }
}