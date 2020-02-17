using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add magic stock to character
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/161_ADDMAGIC"/>
    /// <seealso cref="http://wiki.ffrtt.ru/index.php?title=FF8/Magic_Codes"/>
    public sealed class ADDMAGIC : JsmInstruction
    {
        /// <summary>
        /// Quantity
        /// </summary>
        private IJsmExpression _arg0;
        /// <summary>
        /// Magic ID
        /// </summary>
        private IJsmExpression _arg1;
        /// <summary>
        /// Character ID
        /// </summary>
        private Characters CharacterID;
        
        public ADDMAGIC(IJsmExpression arg0, IJsmExpression arg1, Characters characterid)
        {
            _arg0 = arg0;
            _arg1 = arg1;
            CharacterID = characterid;
        }

        public ADDMAGIC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterid: (Characters)((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDMAGIC)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1}, {nameof(CharacterID)}: {CharacterID})";
        }
    }
}