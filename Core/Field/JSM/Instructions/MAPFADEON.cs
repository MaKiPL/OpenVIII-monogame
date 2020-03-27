namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MAPFADEON : JsmInstruction
    {
        #region Constructors

        public MAPFADEON()
        {
        }

        public MAPFADEON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.FadeOn))
                .Comment(nameof(MAPFADEON));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].FadeOn();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(MAPFADEON)}()";

        #endregion Methods
    }
}