using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Music Vol Sync?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/149_MUSICVOLSYNC&action=edit&redlink=1"/>
    public sealed class MUSICVOLSYNC : JsmInstruction
    {
        public MUSICVOLSYNC()
        {
        }

        public MUSICVOLSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICVOLSYNC)}()";
        }
    }
}