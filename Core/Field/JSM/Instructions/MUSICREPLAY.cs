using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Replay music?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/141_MUSICREPLAY&action=edit&redlink=1"/>
    public sealed class MUSICREPLAY : JsmInstruction
    {
        public MUSICREPLAY()
        {
        }

        public MUSICREPLAY(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICREPLAY)}()";
        }
    }
}