using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class GETCARD : JsmInstruction
    {
        private IJsmExpression _arg0;

        public GETCARD(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public GETCARD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(GETCARD)}({nameof(_arg0)}: {_arg0})";
        }
    }
}