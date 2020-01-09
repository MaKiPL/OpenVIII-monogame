using System;


namespace OpenVIII.Fields
{
    internal sealed class MUSICSTATUS : JsmInstruction
    {
        public MUSICSTATUS()
        {
        }

        public MUSICSTATUS(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICSTATUS)}()";
        }
    }
}