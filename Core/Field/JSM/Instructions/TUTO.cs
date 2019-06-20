using System;


namespace OpenVIII
{
    internal sealed class TUTO : JsmInstruction
    {
        private IJsmExpression _arg0;

        public TUTO(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public TUTO(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(TUTO)}({nameof(_arg0)}: {_arg0})";
        }
    }
}