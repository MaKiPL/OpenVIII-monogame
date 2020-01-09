using System;


namespace OpenVIII.Fields
{
    internal sealed class KILLBAR : JsmInstruction
    {
        private IJsmExpression _arg0;

        public KILLBAR(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public KILLBAR(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(KILLBAR)}({nameof(_arg0)}: {_arg0})";
        }
    }
}