using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add Gil to Team Laguna
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/152_ADDPASTGIL&action=edit&redlink=1"/>
    public sealed class ADDPASTGIL : JsmInstruction
    {
        /// <summary>
        /// Amount of Gil
        /// </summary>
        private IJsmExpression _arg0;

        public ADDPASTGIL(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public ADDPASTGIL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDPASTGIL)}({nameof(_arg0)}: {_arg0})";
        }
    }
}