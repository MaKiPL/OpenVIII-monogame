namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Pushes a random number into temp variable 0 in the range [0-255].
    /// </summary>
    internal sealed class RND : JsmInstruction
    {
        #region Constructors

        public RND()
        {
        }

        public RND(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Properties

        public static ScriptResultId ResultVariable { get; } = new ScriptResultId(0);

        #endregion Properties

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.AppendLine($"R{ResultVariable.ResultId} = {nameof(Rnd)}.{nameof(Rnd.NextByte)}();");

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Interaction[services][ResultVariable] = Rnd.NextByte();

            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(RND)}()";

        #endregion Methods
    }
}