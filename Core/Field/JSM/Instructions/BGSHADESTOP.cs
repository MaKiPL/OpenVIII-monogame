using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// stop shade?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/0D1_BGSHADESTOP&action=edit&redlink=1"/>
    public sealed class BGSHADESTOP : JsmInstruction
    {
        public BGSHADESTOP()
        {
        }

        public BGSHADESTOP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGSHADESTOP)}()";
        }
    }
}