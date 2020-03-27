namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MUSICVOLTRANS : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _flag;
        private readonly IJsmExpression _transitionDuration;
        private readonly IJsmExpression _volume;

        #endregion Fields

        #region Constructors

        public MUSICVOLTRANS(IJsmExpression flag, IJsmExpression transitionDuration, IJsmExpression volume)
        {
            _flag = flag;
            _transitionDuration = transitionDuration;
            _volume = volume;
        }

        public MUSICVOLTRANS(int parameter, IStack<IJsmExpression> stack)
            : this(
                volume: stack.Pop(),
                transitionDuration: stack.Pop(),
                flag: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.ChangeMusicVolume))
                .Argument("volume", _volume)
                .Argument("flag", _flag)
                .Argument("transitionDuration", _transitionDuration)
                .Comment(nameof(MUSICVOL));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].ChangeMusicVolume(
                _volume.Int32(services),
                _flag.Boolean(services),
                _transitionDuration.Int32(services));
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MUSICVOLTRANS)}({nameof(_flag)}: {_flag}, {nameof(_transitionDuration)}: {_transitionDuration}, {nameof(_volume)}: {_volume})";

        #endregion Methods
    }
}