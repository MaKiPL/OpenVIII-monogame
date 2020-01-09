using System;


namespace OpenVIII.Fields
{
    internal sealed class FACEDIR : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;
        private IJsmExpression _arg2;
        private IJsmExpression _arg3;

        public FACEDIR(IJsmExpression arg0, IJsmExpression arg1, IJsmExpression arg2, IJsmExpression arg3)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            _arg2 = arg2;
            _arg3 = arg3;
        }

        public FACEDIR(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg3: stack.Pop(),
                arg2: stack.Pop(),
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(FACEDIR)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1}, {nameof(_arg2)}: {_arg2}, {nameof(_arg3)}: {_arg3})";
        }
    }
}