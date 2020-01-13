using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class OFFSETSYNC : JsmInstruction
    {
        public OFFSETSYNC()
        {
        }

        public OFFSETSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(OFFSETSYNC)}()";
        }
    }
}