using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class DUALMUSIC : JsmInstruction
    {
        private IJsmExpression _arg0;

        public DUALMUSIC(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public DUALMUSIC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DUALMUSIC)}({nameof(_arg0)}: {_arg0})";
        }
    }
}