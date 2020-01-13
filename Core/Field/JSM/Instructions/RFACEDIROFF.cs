using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class RFACEDIROFF : JsmInstruction
    {
        private IJsmExpression _arg0;

        public RFACEDIROFF(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public RFACEDIROFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(RFACEDIROFF)}({nameof(_arg0)}: {_arg0})";
        }
    }
}