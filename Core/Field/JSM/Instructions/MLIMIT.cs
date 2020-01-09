using System;


namespace OpenVIII.Fields
{
    internal sealed class MLIMIT : JsmInstruction
    {
        private IJsmExpression _arg0;

        public MLIMIT(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public MLIMIT(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MLIMIT)}({nameof(_arg0)}: {_arg0})";
        }
    }
}