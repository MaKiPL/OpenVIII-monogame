using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// This has something to do with being in a ladder animation when entering an area. It's used only in two MD Level areas when it detects you've come from another area via ladder.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/180_UNKNOWN15"/>
    public sealed class Unknown15 : JsmInstruction
    {
        /// <summary>
        /// 29 or 30
        /// </summary>
        private IJsmExpression _arg0;

        public Unknown15(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown15(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown15)}({nameof(_arg0)}: {_arg0})";
        }
    }
}