namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Turns on the display of salary alerts.
    /// </summary>
    internal sealed class SARALYDISPON : JsmInstruction
    {
        #region Constructors

        public SARALYDISPON()
        {
        }

        public SARALYDISPON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(ISalaryService))
                .Property(nameof(ISalaryService.IsSalaryAlertEnabled))
                .Assign(true)
                .Comment(nameof(SARALYDISPON));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Salary[services].IsSalaryAlertEnabled = true;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SARALYDISPON)}()";

        #endregion Methods
    }
}