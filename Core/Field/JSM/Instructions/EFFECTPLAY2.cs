using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Play sound effect</para>
    /// <para>Plays a sound effect through the given sound channel. The channel is important because it's the parameter used in SESTOP to halt a specific sound effect (and to prevent multiple counds from silencing each other). AFAIK Channels go up to 2^20 (which is 1048576), so you can theoretically have 20 sounds playing at once. 0 doesn't seem like it's a usable channel, but this is untested.</para>
    /// <para>Note: It seems each area can have a maximum of 32 sounds predefined(meaning sound ID 31 is the highest you can play with this). You have to use EFFECTPLAY to use more than 32 sounds.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/021_EFFECTPLAY2"/>
    public sealed class EFFECTPLAY2 : JsmInstruction
    {
        private readonly Int32 _fieldSoundIndex;
        /// <summary>
        /// Pan (0=left, 255=right)
        /// </summary>
        private IJsmExpression _pan;
        /// <summary>
        /// Volume (0-127)
        /// </summary>
        private IJsmExpression _volume;
        /// <summary>
        /// Channel (must be a power of 2)
        /// </summary>
        private IJsmExpression _channel;

        public EFFECTPLAY2(Int32 fieldSoundIndex, IJsmExpression pan, IJsmExpression volume, IJsmExpression channel)
        {
            _fieldSoundIndex = fieldSoundIndex;
            _pan = pan;
            _volume = volume;
            _channel = channel;
        }

        public EFFECTPLAY2(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter,
                channel: stack.Pop(),
                volume: stack.Pop(),
                pan: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(EFFECTPLAY2)}({nameof(_fieldSoundIndex)}: {_fieldSoundIndex}, {nameof(_pan)}: {_pan}, {nameof(_volume)}: {_volume}, {nameof(_channel)}: {_channel})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            var formatter = sw.Format(formatterContext, services);

            formatter
                .StaticType(nameof(ISoundService))
                .Method(nameof(ISoundService.PlaySound))
                .Argument("fieldSoundIndex", _fieldSoundIndex)
                .Argument("pan", _pan)
                .Argument("volume", _volume)
                .Argument("channel", _channel)
                .Comment(nameof(MUSICLOAD));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Sound[services].PlaySound(_fieldSoundIndex,
                _pan.Int32(services),
                _volume.Int32(services),
                _channel.Int32(services));

            return DummyAwaitable.Instance;
        }
    }
}