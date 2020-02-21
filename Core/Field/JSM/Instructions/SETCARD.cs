using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Card? Card ID and NPC?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15E_SETCARD&action=edit&redlink=1"/>
    public sealed class SETCARD : JsmInstruction
    {
        /// <summary>
        /// Unsure Maybe NPC who has the card?
        /// </summary>
        private readonly IJsmExpression _maybeNPC;
        /// <summary>
        /// I think this is the card id.
        /// </summary>
        private readonly IJsmExpression _cardID; 

        public SETCARD(IJsmExpression maybeNPC, IJsmExpression cardID)
        {
            _maybeNPC = maybeNPC;
            _cardID = cardID;
        }

        public SETCARD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                cardID: stack.Pop(), //can't cast to card ID with out doing something first
                maybeNPC: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETCARD)}({nameof(_maybeNPC)}: {_maybeNPC}, {nameof(_cardID)}: {_cardID})";
        }
    }
}