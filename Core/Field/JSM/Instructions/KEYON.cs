using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Enable Key</para>
    /// <para>Enables certain keys to be pressed after they are otherwise disabled (for example with UCOFF). See some other page for the key values (will edit later)</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/06E_KEYON"/>
    public sealed class KEYON : JsmInstruction
    {
        /// <summary>
        /// key flags, probably the same enum that kernel uses for Zell's attacks.
        /// </summary>
        private IJsmExpression _arg0;

        public KEYON(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public KEYON(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(KEYON)}({nameof(_arg0)}: {_arg0})";
        }
    }
}