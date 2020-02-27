using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Performs no operation.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/000_NOP"/>
    internal sealed class NOP : JsmInstruction
    {
        public NOP()
        {
        }

        public NOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(NOP)}()";
        }
    }
}