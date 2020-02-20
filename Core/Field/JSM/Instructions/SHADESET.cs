using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Shade set?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0B3_SHADESET&action=edit&redlink=1"/>
    public sealed class SHADESET : JsmInstruction
    {
        private IJsmExpression _arg0;

        public SHADESET(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public SHADESET(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SHADESET)}({nameof(_arg0)}: {_arg0})";
        }
    }
}