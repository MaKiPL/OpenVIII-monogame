using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Turn Particle on.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/14E_PARTICLEON&action=edit&redlink=1"/>
    public sealed class PARTICLEON : JsmInstruction
    {
        private IJsmExpression _arg0;

        public PARTICLEON(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public PARTICLEON(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PARTICLEON)}({nameof(_arg0)}: {_arg0})";
        }
    }
}