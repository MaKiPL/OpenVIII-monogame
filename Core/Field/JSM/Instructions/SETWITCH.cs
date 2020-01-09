using System;


namespace OpenVIII.Fields
{
    internal sealed class SETWITCH : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SETWITCH(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SETWITCH(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETWITCH)}({nameof(_arg0)}: {_arg0})";
        }
    }
}