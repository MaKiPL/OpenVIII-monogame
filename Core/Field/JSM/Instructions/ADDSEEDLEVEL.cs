using System;


namespace OpenVIII.Fields
{
    internal sealed class ADDSEEDLEVEL : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ADDSEEDLEVEL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ADDSEEDLEVEL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDSEEDLEVEL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}