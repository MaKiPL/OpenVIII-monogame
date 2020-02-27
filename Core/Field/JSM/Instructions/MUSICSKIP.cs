using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Music skip?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/144_MUSICSKIP&action=edit&redlink=1"/>
    public sealed class MUSICSKIP : JsmInstruction
    {

        /// <summary>
        /// i'm guessing this is time skipped?
        /// </summary>
        private IJsmExpression _arg0;

        public MUSICSKIP(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public MUSICSKIP(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICSKIP)}({nameof(_arg0)}: {_arg0})";
        }
    }
}