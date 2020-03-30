namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add item to party
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/125_ADDITEM"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/Item_Codes"/>
    public sealed class ADDITEM : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _id;
        private readonly IJsmExpression _qty;

        #endregion Fields

        #region Constructors

        public ADDITEM(IJsmExpression id, IJsmExpression qty)
        {
            _id = id;
            _qty = qty;
        }

        public ADDITEM(int parameter, IStack<IJsmExpression> stack)
            : this(
                qty: stack.Pop(),
                id: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override string ToString() => $"{nameof(ADDITEM)}({nameof(_id)}: {_id}, {nameof(_qty)}: {_qty}";

        #endregion Methods
    }
}