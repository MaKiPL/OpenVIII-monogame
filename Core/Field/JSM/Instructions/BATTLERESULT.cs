using System;


namespace OpenVIII.Fields
{
    internal sealed class BATTLERESULT : JsmInstruction
    {
        public BATTLERESULT()
        {
        }

        public BATTLERESULT(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BATTLERESULT)}()";
        }
    }
}