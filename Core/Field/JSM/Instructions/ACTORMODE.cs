using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Controls transparency/blinking effects on models (might also control whether Squall/Seifer's gunblades are visible).
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/12D_ACTORMODE"/>
    public sealed class ACTORMODE : JsmInstruction
    {
        /// <summary>
        /// Model BattleID?
        /// </summary>
        private IJsmExpression _arg0;

        public ACTORMODE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ACTORMODE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ACTORMODE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}