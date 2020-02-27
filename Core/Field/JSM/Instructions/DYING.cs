using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Dying: This is used a lot whenever the party changes members or goes to/from the dream world.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/145_DYING"/>
    public sealed class DYING : JsmInstruction
    {
        public DYING()
        {
        }

        public DYING(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(DYING)}()";
        }
    }
}