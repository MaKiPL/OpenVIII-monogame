using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Swap?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/162_SWAP&action=edit&redlink=1"/>
    public sealed class SWAP : JsmInstruction
    {
        public SWAP()
        {
        }

        public SWAP(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SWAP)}()";
        }
    }
}