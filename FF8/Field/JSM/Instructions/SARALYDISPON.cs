using System;

namespace FF8
{
    /// <summary>
    /// Turns on the display of salary alerts.
    /// </summary>
    internal sealed class SARALYDISPON : JsmInstruction
    {
        public SARALYDISPON()
        {
        }

        public SARALYDISPON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SARALYDISPON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(ISalaryService))
                .Property(nameof(ISalaryService.IsSalaryAlertEnabled))
                .Assign(true)
                .Comment(nameof(SARALYDISPON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Salary[services].IsSalaryAlertEnabled = true;
            return DummyAwaitable.Instance;
        }
    }
}