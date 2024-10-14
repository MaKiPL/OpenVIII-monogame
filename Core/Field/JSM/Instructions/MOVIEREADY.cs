using OpenVIII.Battle;
using System;
using static OpenVIII.Fields.Scripts.Jsm.Expression;
//@see https://wiki.ffrtt.ru/index.php/FF8/Field/Script/Opcodes/0A3_MOVIEREADY
namespace OpenVIII.Fields.Scripts.Instructions
{
    public sealed class MOVIEREADY : JsmInstruction
    {
        #region Fields

        private readonly ushort _flag; // either 0 or 1.
        private readonly ushort _movieId;

        #endregion Fields

        #region Constructors

        public MOVIEREADY(IJsmExpression movieId, IJsmExpression flag)
        {
            if (movieId is PSHN_L)
                _movieId = ((IConstExpression)movieId).UInt16();
            else
            {
                //requires service to evaluate value?
                _movieId = ushort.MaxValue;//so I set an invalid value here.
            }
            _flag = ((IConstExpression)flag).UInt16();
            
        }

        public MOVIEREADY(int parameter, IStack<IJsmExpression> stack)
            : this(
                flag: stack.Pop(),
                movieId: stack.Pop())
        {
        }

        public ushort Flag => _flag;

        public ushort MovieId => _movieId;

        #endregion Constructors

        #region Methods

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

        public override string ToString() => $"{nameof(MOVIEREADY)}({nameof(_movieId)}: {_movieId}, {nameof(_flag)}: {_flag})";

        #endregion Methods
    }
}