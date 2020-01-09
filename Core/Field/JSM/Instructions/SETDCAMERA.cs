using System;


namespace OpenVIII.Fields
{
    internal sealed class SETDCAMERA : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SETDCAMERA(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SETDCAMERA(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETDCAMERA)}({nameof(_arg0)}: {_arg0})";
        }
    }
}