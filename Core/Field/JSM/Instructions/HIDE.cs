namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Hides this entity's model on the field. See also SHOW.
    /// </summary>
    internal sealed class HIDE : JsmInstruction
    {
        #region Constructors

        public HIDE()
        {
        }

        public HIDE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Hide))
                .Comment(nameof(HIDE));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.Hide();
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(HIDE)}()";

        #endregion Methods
    }
}