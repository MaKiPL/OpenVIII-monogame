using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes to blink? Unused!
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_BLINKEYES&action=edit&redlink=1"/>
    public sealed class BLINKEYES : JsmInstruction
    {
        public BLINKEYES()
        {
        }

        public BLINKEYES(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BLINKEYES)}()";
        }
    }
}