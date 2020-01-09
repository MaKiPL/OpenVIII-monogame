using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class ADDGIL : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ADDGIL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ADDGIL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDGIL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}