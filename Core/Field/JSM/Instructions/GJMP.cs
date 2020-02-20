using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// GJMP is unused.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/004_GJMP&action=edit&redlink=1"/>
    internal sealed class GJMP : JsmInstruction
    {
        public GJMP()
        {
            throw new NotSupportedException();
        }
    }
}