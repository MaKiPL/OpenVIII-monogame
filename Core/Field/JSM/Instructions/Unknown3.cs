using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class Unknown3 : JsmInstruction
    {
        private IJsmExpression _arg0;

        public Unknown3(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown3(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown3)}({nameof(_arg0)}: {_arg0})";
        }
    }
}