using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Force Character's eyes open? Unused!
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/15C_OPENEYES&action=edit&redlink=1"/>
    public sealed class OPENEYES : JsmInstruction
    {
        public OPENEYES()
        {
        }

        public OPENEYES(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(OPENEYES)}()";
        }
    }
}