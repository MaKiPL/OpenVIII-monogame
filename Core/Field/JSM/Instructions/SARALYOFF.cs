using System;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Disables the payment of salaries.
    /// </summary>
    internal sealed class SARALYOFF : JsmInstruction
    {
        public SARALYOFF()
        {
        }

        public SARALYOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SARALYOFF)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(ISalaryService))
                .Property(nameof(ISalaryService.IsSalaryEnabled))
                .Assign(false)
                .Comment(nameof(SARALYOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Salary[services].IsSalaryEnabled = false;
            return DummyAwaitable.Instance;
        }
    }
}