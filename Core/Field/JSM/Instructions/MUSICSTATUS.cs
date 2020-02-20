using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// See if music is playing?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/140_MUSICSTATUS&action=edit&redlink=1"/>
    public sealed class MUSICSTATUS : JsmInstruction
    {
        public MUSICSTATUS()
        {
        }

        public MUSICSTATUS(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICSTATUS)}()";
        }
    }
}