using System;


namespace OpenVIII
{
    internal sealed class GETINFO : JsmInstruction
    {
        public GETINFO()
        {
        }

        public GETINFO(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(GETINFO)}()";
        }
    }
}