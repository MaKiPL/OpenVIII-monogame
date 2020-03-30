namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Preloads a new field music track. You can start the new track by calling MUSICCHANGE.
    /// </summary>
    internal sealed class MUSICLOAD : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _musicId;

        #endregion Fields

        #region Constructors

        public MUSICLOAD(IJsmExpression musicId) => _musicId = musicId;

        public MUSICLOAD(int parameter, IStack<IJsmExpression> stack)
            : this(
                musicId: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            var formatter = sw.Format(formatterContext, services);

            if (_musicId is IConstExpression expr)
                formatter.CommentLine(MusicName.Get((MusicId)expr.Int32()));

            formatter
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.LoadFieldMusic))
                .Argument("musicId", _musicId)
                .Comment(nameof(MUSICLOAD));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].LoadFieldMusic((MusicId)_musicId.Int32(services));
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MUSICLOAD)}({nameof(_musicId)}: {_musicId})";

        #endregion Methods
    }
}