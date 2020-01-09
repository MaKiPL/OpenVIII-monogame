using System;


namespace OpenVIII.Fields
{
    internal sealed class FADESYNC : JsmInstruction
    {
        public FADESYNC()
        {
        }

        public FADESYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FADESYNC)}()";
        }
    }
}