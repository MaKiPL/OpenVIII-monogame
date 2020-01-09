using System;


namespace OpenVIII.Fields
{
    internal sealed class BATTLEMODE : JsmInstruction
    {
        private IJsmExpression _arg0;

        public BATTLEMODE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public BATTLEMODE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BATTLEMODE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}