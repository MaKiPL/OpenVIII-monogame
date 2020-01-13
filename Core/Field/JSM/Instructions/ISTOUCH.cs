using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class ISTOUCH : JsmInstruction
    {
        private IJsmExpression _arg0;

        public ISTOUCH(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ISTOUCH(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ISTOUCH)}({nameof(_arg0)}: {_arg0})";
        }
    }
}