using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Where Card? I guess this is who has a rare card.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/160_WHERECARD&action=edit&redlink=1"/>
    public sealed class WHERECARD : JsmInstruction
    {
        /// <summary>
        /// card BattleID?
        /// </summary>
        private IJsmExpression _cardID;

        public WHERECARD(IJsmExpression cardID)
        {
            _cardID = cardID;
        }

        public WHERECARD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                cardID: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(WHERECARD)}({nameof(_cardID)}: {_cardID})";
        }
    }
}