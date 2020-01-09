using System;


namespace OpenVIII.Fields
{
    internal sealed class RMOVEA : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public RMOVEA(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public RMOVEA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(RMOVEA)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}