using System;


namespace OpenVIII.Fields
{
    internal sealed class IDUNLOCK : JsmInstruction
    {
        private Int32 _parameter;

        public IDUNLOCK(Int32 parameter)
        {
            _parameter = parameter;
        }

        public IDUNLOCK(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(IDUNLOCK)}({nameof(_parameter)}: {_parameter})";
        }
    }
}