using System;


namespace OpenVIII.Fields
{
    internal sealed class TALKRADIUS : JsmInstruction
    {
        private IJsmExpression _arg0;

        public TALKRADIUS(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public TALKRADIUS(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(TALKRADIUS)}({nameof(_arg0)}: {_arg0})";
        }
    }
}