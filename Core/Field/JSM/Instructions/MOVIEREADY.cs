using System;

namespace OpenVIII.Fields
{
    internal sealed class MOVIEREADY : JsmInstruction
    {
        private Int32 _movieId;
        private Boolean _flag;

        public MOVIEREADY(Int32 movieId, Boolean flag)
        {
            _movieId = movieId;
            _flag = flag;
        }

        public MOVIEREADY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                flag: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean(),
                movieId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVIEREADY)}({nameof(_movieId)}: {_movieId}, {nameof(_flag)}: {_flag})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            var formatter = sw.Format(formatterContext, services);

            foreach (String name in MovieName.PossibleNames(_movieId))
                formatter.CommentLine(name);

            formatter
                .StaticType(nameof(IMovieService))
                .Method(nameof(IMovieService.PrepareToPlay))
                .Argument("movieId", _movieId)
                .Argument("flag", _flag)
                .Comment(nameof(MOVIEREADY));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Movie[services].PrepareToPlay(_movieId, _flag);
            return DummyAwaitable.Instance;
        }
    }
}