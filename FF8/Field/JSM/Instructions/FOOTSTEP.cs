using System;


namespace FF8
{
    internal sealed class FOOTSTEP : JsmInstruction
    {
        private Int32 _parameter;
        private IJsmExpression _arg0;

        public FOOTSTEP(Int32 parameter, IJsmExpression arg0)
        {
            _parameter = parameter;
            _arg0 = arg0;
        }

        public FOOTSTEP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter,
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEP)}({nameof(_parameter)}: {_parameter}, {nameof(_arg0)}: {_arg0})";
        }
    }
}