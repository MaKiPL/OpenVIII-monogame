using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class RESETGF : JsmInstruction
    {
        private IJsmExpression _arg0;

        public RESETGF(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public RESETGF(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(RESETGF)}({nameof(_arg0)}: {_arg0})";
        }
    }
}