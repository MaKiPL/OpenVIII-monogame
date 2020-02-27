using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Sound channel available?</para>
    /// <para>Used in Galbadian Missile Base when pushing the launcher into its bay. It's supposed to control which channel the pushing sound plays through. However, it doesn't seem to actually do anything.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/178_UNKNOWN13"/>
    public sealed class Unknown13 : JsmInstruction
    {
        /// <summary>
        /// Sound channel?
        /// </summary>
        private IJsmExpression _arg0;

        public Unknown13(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public Unknown13(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown13)}({nameof(_arg0)}: {_arg0})";
        }
    }
}