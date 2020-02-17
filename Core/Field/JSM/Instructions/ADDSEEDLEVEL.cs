using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add to seed Level
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/153_ADDSEEDLEVEL&action=edit&redlink=1"/>
    public sealed class ADDSEEDLEVEL : JsmInstruction
    {
        /// <summary>
        /// Amount
        /// </summary>
        private IJsmExpression _arg0;

        public ADDSEEDLEVEL(IJsmExpression arg0)
        {
            _arg0 = arg0; //100,60,40,-100,150, GlobalValue
        }

        public ADDSEEDLEVEL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDSEEDLEVEL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}