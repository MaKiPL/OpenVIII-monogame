using System;


namespace OpenVIII
{
    internal sealed class EFFECTPLAY2 : JsmInstruction
    {
        private readonly Int32 _fieldSoundIndex;
        private IJsmExpression _pan;
        private IJsmExpression _volume;
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