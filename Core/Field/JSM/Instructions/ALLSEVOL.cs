using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Volume of all Sound Effects
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0C3_ALLSEVOL"/>
    public sealed class ALLSEVOL : JsmInstruction
    {
        /// <summary>
        /// Volume (0-127)
        /// </summary>
        private IJsmExpression _arg0;

        public ALLSEVOL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ALLSEVOL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ALLSEVOL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}