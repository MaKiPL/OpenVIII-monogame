using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Push animation on stack?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/14A_PUSHANIME&action=edit&redlink=1"/>
    public sealed class PUSHANIME : JsmInstruction
    {
        public PUSHANIME()
        {
        }

        public PUSHANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(PUSHANIME)}()";
        }
    }
}