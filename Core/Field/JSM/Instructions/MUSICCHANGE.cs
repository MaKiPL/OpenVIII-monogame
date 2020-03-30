namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MUSICCHANGE : JsmInstruction
    {
        #region Constructors

        public MUSICCHANGE()
        {
        }

        public MUSICCHANGE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.PlayFieldMusic))
                .Comment(nameof(MUSICCHANGE));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].PlayFieldMusic();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MUSICCHANGE)}()";

        #endregion Methods
    }
}