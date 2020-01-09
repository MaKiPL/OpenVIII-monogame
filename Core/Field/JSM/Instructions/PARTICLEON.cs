using System;


namespace OpenVIII.Fields
{
    internal sealed class PARTICLEON : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PARTICLEON(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PARTICLEON(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PARTICLEON)}({nameof(_arg0)}: {_arg0})";
        }
    }
}