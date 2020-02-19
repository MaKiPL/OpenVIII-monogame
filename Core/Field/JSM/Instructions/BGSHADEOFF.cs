using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// turn off shade?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/117_BGSHADEOFF&action=edit&redlink=1"/>
    public sealed class BGSHADEOFF : JsmInstruction
    {
        public BGSHADEOFF()
        {
        }

        public BGSHADEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGSHADEOFF)}()";
        }
    }
}