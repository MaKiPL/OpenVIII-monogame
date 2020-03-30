namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Where Card? I guess this is who has a rare card.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/160_WHERECARD&action=edit&redlink=1"/>
    public sealed class WHERECARD : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// card ID?
        /// </summary>
        private readonly IJsmExpression _cardID;

        #endregion Fields

        #region Constructors

        public WHERECARD(IJsmExpression cardID) => _cardID = cardID;

        public WHERECARD(int parameter, IStack<IJsmExpression> stack)
            : this(
                cardID: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(WHERECARD)}({nameof(_cardID)}: {_cardID})";

        #endregion Methods
    }
}