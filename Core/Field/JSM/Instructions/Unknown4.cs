using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Used only twice, on the Ragnarok hatch screen.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/169_UNKNOWN4"/>
    public sealed class Unknown4 : JsmInstruction
    {
        /// <summary>
        /// 1
        /// </summary>
        private IJsmExpression _arg0;

        public Unknown4(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown4(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown4)}({nameof(_arg0)}: {_arg0})";
        }
    }
}