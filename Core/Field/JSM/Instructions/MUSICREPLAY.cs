using System;


namespace OpenVIII.Fields
{
    internal sealed class MUSICREPLAY : JsmInstruction
    {
        public MUSICREPLAY()
        {
        }

        public MUSICREPLAY(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICREPLAY)}()";
        }
    }
}