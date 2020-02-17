using System;
using static OpenVIII.Fields.Scripts.Jsm.Expression;

namespace OpenVIII.Fields.Scripts.Instructions
{
    public sealed class BATTLE : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public BATTLE(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public BATTLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }
        public ushort Encounter => checked((ushort)((PSHN_L)_arg0).Value);
        public override String ToString()
        {
            return $"{nameof(BATTLE)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}