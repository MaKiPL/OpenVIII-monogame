using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MUSICVOLTRANS : JsmInstruction
    {
        private IJsmExpression _flag;
        private IJsmExpression _transitionDuration;
        private IJsmExpression _volume;

        public MUSICVOLTRANS(IJsmExpression flag, IJsmExpression transitionDuration, IJsmExpression volume)
        {
            _flag = flag;
            _transitionDuration = transitionDuration;
            _volume = volume;
        }

        public MUSICVOLTRANS(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                volume: stack.Pop(),
                transitionDuration: stack.Pop(),
                flag: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICVOLTRANS)}({nameof(_flag)}: {_flag}, {nameof(_transitionDuration)}: {_transitionDuration}, {nameof(_volume)}: {_volume})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.ChangeMusicVolume))
                .Argument("volume", _volume)
                .Argument("flag", _flag)
                .Argument("transitionDuration", _transitionDuration)
                .Comment(nameof(MUSICVOL));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].ChangeMusicVolume(
                _volume.Int32(services),
                _flag.Boolean(services),
                _transitionDuration.Int32(services));
            return DummyAwaitable.Instance;
        }
    }
}