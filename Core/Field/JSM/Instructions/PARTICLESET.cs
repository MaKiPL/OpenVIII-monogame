using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set Particle?
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/154_PARTICLESET&action=edit&redlink=1"/>
    public sealed class PARTICLESET : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PARTICLESET(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PARTICLESET(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PARTICLESET)}({nameof(_arg0)}: {_arg0})";
        }
    }
}