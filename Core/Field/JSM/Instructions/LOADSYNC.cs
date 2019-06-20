using System;


namespace OpenVIII
{
    internal sealed class LOADSYNC : JsmInstruction
    {
        public LOADSYNC()
        {
        }

        public LOADSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(LOADSYNC)}()";
        }
    }
}