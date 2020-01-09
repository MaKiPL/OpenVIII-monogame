using System;


namespace OpenVIII.Fields
{
    internal sealed class BGSHADESTOP : JsmInstruction
    {
        public BGSHADESTOP()
        {
        }

        public BGSHADESTOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGSHADESTOP)}()";
        }
    }
}