using System;


namespace OpenVIII.Fields
{
    internal sealed class ALLSEVOL : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ALLSEVOL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ALLSEVOL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ALLSEVOL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}