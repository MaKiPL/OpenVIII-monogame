using System;


namespace OpenVIII
{
    internal sealed class MOVESYNC : JsmInstruction
    {
        public MOVESYNC()
        {
        }

        public MOVESYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVESYNC)}()";
        }
    }
}