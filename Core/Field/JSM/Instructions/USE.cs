namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Opposite of UNUSE.
    /// </summary>
    internal sealed class USE : JsmInstruction
    {
        #region Constructors

        public USE()
        {
        }

        public USE(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.IsActive))
                .Assign(true)
                .Comment(nameof(USE));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.IsActive = true;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(USE)}()";

        #endregion Methods
    }
}