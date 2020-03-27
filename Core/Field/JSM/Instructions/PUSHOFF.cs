namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Disables this entity's "push" script. See PUSHON.
    /// </summary>
    internal sealed class PUSHOFF : JsmInstruction
    {
        #region Constructors

        public PUSHOFF()
        {
        }

        public PUSHOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.IsPushScriptActive))
                .Assign(false)
                .Comment(nameof(PUSHOFF));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.IsPushScriptActive = false;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(PUSHOFF)}()";

        #endregion Methods
    }
}