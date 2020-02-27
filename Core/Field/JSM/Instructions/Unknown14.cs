using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Preserve Sound Channel</para>
    /// <para>Prevents the given sound channel from being silenced when a new field is loaded. The currently playing sound effect will continue to play during the fade.</para>
    /// <para>Does not work on channel 0.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/179_UNKNOWN14"/>
    public sealed class Unknown14 : JsmInstruction
    {
        /// <summary>
        /// Sound channel
        /// </summary>
        private IJsmExpression _soundChannel;

        public Unknown14(IJsmExpression soundChannel)
        {
            _soundChannel = soundChannel;
        }

        public Unknown14(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                soundChannel: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(Unknown14)}({nameof(_soundChannel)}: {_soundChannel})";
        }
    }
}