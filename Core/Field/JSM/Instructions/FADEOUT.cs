namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADEOUT : JsmInstruction
    {
        #region Constructors

        public FADEOUT()
        {
        }

        public FADEOUT(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.FadeOut))
                .Comment(nameof(FADEOUT));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].FadeOut();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(FADEOUT)}()";

        #endregion Methods
    }
}