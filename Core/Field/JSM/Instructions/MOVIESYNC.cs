namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Pauses execution of this script until the current FMV movie is finished playing.
    /// </summary>
    internal sealed class MOVIESYNC : JsmInstruction
    {
        #region Constructors

        public MOVIESYNC()
        {
        }

        public MOVIESYNC(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IMovieService))
                .Method(nameof(IMovieService.Wait))
                .Comment(nameof(MOVIE));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Movie[services].Wait();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MOVIESYNC)}()";

        #endregion Methods
    }
}