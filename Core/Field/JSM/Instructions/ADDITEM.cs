using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add item to party
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/125_ADDITEM"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/Item_Codes"/>
    public sealed class ADDITEM : JsmInstruction
    {
        private readonly IJsmExpression _id;
        private readonly IJsmExpression _qty;

        public ADDITEM(IJsmExpression id, IJsmExpression qty)
        {
            _id = id;
            _qty = qty;
        }

        public ADDITEM(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                qty: stack.Pop(),
                id: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDITEM)}({nameof(_id)}: {_id}, {nameof(_qty)}: {_qty}";
        }
    }
}