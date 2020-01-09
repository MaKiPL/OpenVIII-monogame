using System;


namespace OpenVIII.Fields
{
    internal sealed class Unknown16 : JsmInstruction
    {
        private IJsmExpression _arg0;

        public Unknown16(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown16(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown16)}({nameof(_arg0)}: {_arg0})";
        }
    }
}