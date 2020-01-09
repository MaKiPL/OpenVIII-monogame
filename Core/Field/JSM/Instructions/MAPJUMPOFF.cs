using System;


namespace OpenVIII.Fields
{
    internal sealed class MAPJUMPOFF : JsmInstruction
    {
        public MAPJUMPOFF()
        {
        }

        public MAPJUMPOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MAPJUMPOFF)}()";
        }
    }
}