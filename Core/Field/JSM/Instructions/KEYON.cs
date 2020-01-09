using System;


namespace OpenVIII.Fields
{
    internal sealed class KEYON : JsmInstruction
    {
        private IJsmExpression _arg0;

        public KEYON(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public KEYON(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEYON)}({nameof(_arg0)}: {_arg0})";
        }
    }
}