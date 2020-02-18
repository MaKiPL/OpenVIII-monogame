using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// pop animation off stack?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/14B_POPANIME&action=edit&redlink=1"/>
    public sealed class POPANIME : JsmInstruction
    {
        public POPANIME()
        {
        }

        public POPANIME(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(POPANIME)}()";
        }
    }
}