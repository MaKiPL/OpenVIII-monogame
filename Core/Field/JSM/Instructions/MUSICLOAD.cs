using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Preloads a new field music track. You can start the new track by calling MUSICCHANGE. 
    /// </summary>
    internal sealed class MUSICLOAD : JsmInstruction
    {
        private readonly IJsmExpression _musicId;

        public MUSICLOAD(IJsmExpression musicId)
        {
            _musicId = musicId;
        }

        public MUSICLOAD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                musicId: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICLOAD)}({nameof(_musicId)}: {_musicId})";
        }

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
    }
}