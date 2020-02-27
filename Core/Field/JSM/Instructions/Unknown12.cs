using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// End Unknown11
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/177_UNKNOWN12"/>
    public sealed class Unknown12 : JsmInstruction
    {
        public Unknown12()
        {
        }

        public Unknown12(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown12)}()";
        }
    }
}