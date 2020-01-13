using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GETPARTY : JsmInstruction
    {
        private IJsmExpression _arg0;

        public GETPARTY(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public GETPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(GETPARTY)}({nameof(_arg0)}: {_arg0})";
        }
    }
}