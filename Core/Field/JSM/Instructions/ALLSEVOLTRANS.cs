using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Transition Volume of all Sound Effects
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0C4_ALLSEVOLTRANS"/>
    public sealed class ALLSEVOLTRANS : JsmInstruction
    {
        /// <summary>
        /// Final Volume (0-127)
        /// </summary>
        private IJsmExpression _arg0;
        /// <summary>
        /// Frame count
        /// </summary>
        private IJsmExpression _arg1;

        public ALLSEVOLTRANS(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public ALLSEVOLTRANS(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ALLSEVOLTRANS)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}