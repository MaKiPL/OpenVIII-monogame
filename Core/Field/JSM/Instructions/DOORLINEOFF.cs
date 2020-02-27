using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// door line off? disable door?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/142_DOORLINEOFF&action=edit&redlink=1"/>
    public sealed class DOORLINEOFF : JsmInstruction
    {
        public DOORLINEOFF()
        {
        }

        public DOORLINEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(DOORLINEOFF)}()";
        }
    }
}