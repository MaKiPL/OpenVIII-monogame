namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SHOW : JsmInstruction
    {
        #region Constructors

        public SHOW()
        {
        }

        public SHOW(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Show))
                .Comment(nameof(SHOW));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.Show();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SHOW)}()";

        #endregion Methods
    }
}