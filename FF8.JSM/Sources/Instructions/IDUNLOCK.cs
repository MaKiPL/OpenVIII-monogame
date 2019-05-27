using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
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