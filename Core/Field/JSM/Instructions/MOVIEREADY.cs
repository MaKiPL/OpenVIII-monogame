using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MOVIEREADY : JsmInstruction
    {
        private IJsmExpression _movieId;
        private IJsmExpression _flag;

        public MOVIEREADY(IJsmExpression movieId, IJsmExpression flag)
        {
            _movieId = movieId;
            _flag = flag;
        }

        public MOVIEREADY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                flag: stack.Pop(),
                movieId: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVIEREADY)}({nameof(_movieId)}: {_movieId}, {nameof(_flag)}: {_flag})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            var formatter = sw.Format(formatterContext, services);

            //foreach (String name in MovieName.PossibleNames(_movieId))
            //    formatter.CommentLine(name);

            formatter
                .StaticType(nameof(IMovieService))
                .Method(nameof(IMovieService.PrepareToPlay))
                .Argument("movieId", _movieId)
                .Argument("flag", _flag)
                .Comment(nameof(MOVIEREADY));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            throw new NotImplementedException("was getting InvalidCastExceptions So need correct cast to movieid and flag");
            //ServiceId.Movie[services].PrepareToPlay(_movieId, _flag);
            return DummyAwaitable.Instance;
        }
    }
}