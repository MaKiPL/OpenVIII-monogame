using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Flush Movement; Not confirmed, but I'm pretty sure it halts the current entity's movements.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/148_MOVEFLUSH"/>
    public sealed class MOVEFLUSH : JsmInstruction
    {
        public MOVEFLUSH()
        {
        }

        public MOVEFLUSH(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVEFLUSH)}()";
        }
    }
}