using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes closed
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_CLOSEEYES&action=edit&redlink=1"/>
    public sealed class CLOSEEYES : JsmInstruction
    {
        public CLOSEEYES()
        {
        }

        public CLOSEEYES(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(CLOSEEYES)}()";
        }
    }
}