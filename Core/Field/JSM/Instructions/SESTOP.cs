using System;


namespace OpenVIII.Fields
{
    internal sealed class SESTOP : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SESTOP(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SESTOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SESTOP)}({nameof(_arg0)}: {_arg0})";
        }
    }
}