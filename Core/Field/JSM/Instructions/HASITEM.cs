using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Has item (or Get Item Count?)</para>
    /// <para>If the party has the item with the given ID, pushes 1 to temporary variable 0. Otherwise, pushes 0.</para>
    /// <para>It's possible this just returns the number of the item the party has.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/170_UNKNOWN5"/>
    public sealed class HASITEM : JsmInstruction
    {
        /// <summary>
        /// Item ID
        /// </summary>
        private IJsmExpression _itemID;

        public HASITEM(IJsmExpression itemID)
        {
            _itemID = itemID;
        }

        public HASITEM(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                itemID: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(HASITEM)}({nameof(_itemID)}: {_itemID})";
        }
    }
}