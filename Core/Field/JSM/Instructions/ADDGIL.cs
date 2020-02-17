using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add Gil
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/151_ADDGIL&action=edit&redlink=1"/>
    public sealed class ADDGIL : JsmInstruction
    {
        /// <summary>
        /// Ammount of gil?
        /// </summary>
        private IJsmExpression _arg0;

        public ADDGIL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ADDGIL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDGIL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}