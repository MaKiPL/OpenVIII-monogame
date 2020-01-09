using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Set the volume of the music.
    /// All the music functions have a parameter that's either 0 or 1. 
    /// </summary>
    internal sealed class MUSICVOL : JsmInstruction
    {
        private IJsmExpression _flag;
        private IJsmExpression _volume;

        public MUSICVOL(IJsmExpression flag, IJsmExpression volume)
        {
            _flag = flag;
            _volume = volume;
        }

        public MUSICVOL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                volume: stack.Pop(),
                flag: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICVOL)}({nameof(_flag)}: {_flag}, {nameof(_volume)}: {_volume})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.ChangeMusicVolume))
                .Argument("volume", _volume)
                .Argument("flag", _flag)
                .Comment(nameof(MUSICVOL));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].ChangeMusicVolume(
                _volume.Int32(services),
                _flag.Boolean(services));
            return DummyAwaitable.Instance;
        }
    }
}