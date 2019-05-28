using System;
using FF8.Framework;

namespace FF8.JSM.Instructions
{
    internal sealed class IDLOCK : JsmInstruction
    {
        private Int32 _parameter;

        public IDLOCK(Int32 parameter)
        {
            _parameter = parameter;
        }

        public IDLOCK(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(IDLOCK)}({nameof(_parameter)}: {_parameter})";
        }
    }
}