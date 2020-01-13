using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class JUNCTION : JsmInstruction
    {
        private IJsmExpression _arg0;

        public JUNCTION(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public JUNCTION(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(JUNCTION)}({nameof(_arg0)}: {_arg0})";
        }
    }
}