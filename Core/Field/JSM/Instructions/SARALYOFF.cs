namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Disables the payment of salaries.
    /// </summary>
    internal sealed class SARALYOFF : JsmInstruction
    {
        #region Constructors

        public SARALYOFF()
        {
        }

        public SARALYOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(ISalaryService))
                .Property(nameof(ISalaryService.IsSalaryEnabled))
                .Assign(false)
                .Comment(nameof(SARALYOFF));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Salary[services].IsSalaryEnabled = false;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SARALYOFF)}()";

        #endregion Methods
    }
}