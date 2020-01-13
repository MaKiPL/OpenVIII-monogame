using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class PDIRA : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PDIRA(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PDIRA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PDIRA)}({nameof(_arg0)}: {_arg0})";
        }
    }
}