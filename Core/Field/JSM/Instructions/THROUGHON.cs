using System;


namespace OpenVIII.Fields
{
    internal sealed class THROUGHON : JsmInstruction
    {
        public THROUGHON()
        {
        }

        public THROUGHON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(THROUGHON)}()";
        }
    }
}