namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADEIN : JsmInstruction
    {
        #region Constructors

        public FADEIN()
        {
        }

        public FADEIN(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.FadeIn))
                .Comment(nameof(FADEIN));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].FadeIn();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(FADEIN)}()";

        #endregion Methods
    }
}