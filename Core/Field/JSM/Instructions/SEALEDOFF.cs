using System;


namespace OpenVIII.Fields
{
    internal sealed class SEALEDOFF : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SEALEDOFF(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SEALEDOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SEALEDOFF)}({nameof(_arg0)}: {_arg0})";
        }
    }
}