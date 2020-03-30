namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Card? Card ID and NPC?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15E_SETCARD&action=edit&redlink=1"/>
    public sealed class SETCARD : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// I think this is the card ID.
        /// </summary>
        private readonly IJsmExpression _cardID;

        /// <summary>
        /// Unsure Maybe NPC who has the card?
        /// </summary>
        private readonly IJsmExpression _maybeNPC;

        #endregion Fields

        #region Constructors

        public SETCARD(IJsmExpression maybeNPC, IJsmExpression cardID)
        {
            _maybeNPC = maybeNPC;
            _cardID = cardID;
        }

        public SETCARD(int parameter, IStack<IJsmExpression> stack)
            : this(
                cardID: stack.Pop(), //can't cast to card ID with out doing something first
                maybeNPC: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(SETCARD)}({nameof(_maybeNPC)}: {_maybeNPC}, {nameof(_cardID)}: {_cardID})";

        #endregion Methods
    }
}