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
        private Saves.Item _item;

        public ADDITEM(Saves.Item item)
        {
            _item = item;
        }

        public ADDITEM(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                item: new Saves.Item(QTY: checked((byte)((Jsm.Expression.PSHN_L)stack.Pop()).Int32()),
                ID: checked((byte)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())))
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDITEM)}({nameof(_item)}: {_item})";
        }
    }
}