using System;
using OpenVIII.Fields.Scripts.Instructions.Abstract;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Key
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/139_KEY&action=edit&redlink=1"/>
    public sealed class KEY : Abstract.KEY
    {
        public KEY(KeyFlags flags) : base(flags)
        {
        }

        public KEY(int parameter, IStack<IJsmExpression> stack) : base(parameter, stack)
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEY)}({nameof(_flags)}: {_flags})";
        }
    }
}