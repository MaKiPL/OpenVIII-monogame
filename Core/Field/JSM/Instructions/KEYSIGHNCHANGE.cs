using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Keysighnchange, only used on test
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/150_KEYSIGHNCHANGE&action=edit&redlink=1"/>
    public sealed class KEYSIGHNCHANGE : JsmInstruction
    {
        private IJsmExpression _arg0;

        public KEYSIGHNCHANGE(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public KEYSIGHNCHANGE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEYSIGHNCHANGE)}({nameof(_arg0)}: {_arg0})";
        }
    }
}