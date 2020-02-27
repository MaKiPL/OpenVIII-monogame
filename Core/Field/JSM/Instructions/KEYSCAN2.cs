using System;
using OpenVIII.Fields.Scripts.Instructions.Abstract;

namespace OpenVIII.Fields.Scripts.Instructions
{
    public sealed class KEYSCAN2 : Abstract.KEY
    {
        public KEYSCAN2(KeyFlags flags) : base(flags)
        {
        }

        public KEYSCAN2(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEYSCAN2)}({nameof(_flags)}: {_flags})";
        }
    }
}