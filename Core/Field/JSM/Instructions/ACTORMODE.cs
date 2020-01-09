using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class ACTORMODE : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ACTORMODE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ACTORMODE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ACTORMODE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}