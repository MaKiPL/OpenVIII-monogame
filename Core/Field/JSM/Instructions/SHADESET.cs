using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SHADESET : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SHADESET(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SHADESET(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SHADESET)}({nameof(_arg0)}: {_arg0})";
        }
    }
}