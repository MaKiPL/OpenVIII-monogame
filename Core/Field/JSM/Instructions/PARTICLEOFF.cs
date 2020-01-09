using System;


namespace OpenVIII.Fields
{
    internal sealed class PARTICLEOFF : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PARTICLEOFF(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PARTICLEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PARTICLEOFF)}({nameof(_arg0)}: {_arg0})";
        }
    }
}