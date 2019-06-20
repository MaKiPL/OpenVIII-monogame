using System;


namespace OpenVIII
{
    internal sealed class COPYINFO : JsmInstruction
    {
        private IJsmExpression _arg0;

        public COPYINFO(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public COPYINFO(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(COPYINFO)}({nameof(_arg0)}: {_arg0})";
        }
    }
}