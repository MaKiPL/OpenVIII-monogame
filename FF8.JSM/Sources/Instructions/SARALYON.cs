using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Enables the payment of salaries.
    /// </summary>
    internal sealed class SARALYON : JsmInstruction
    {
        public SARALYON()
        {
        }

        public SARALYON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SARALYON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(ISalaryService))
                .Property(nameof(ISalaryService.IsSalaryEnabled))
                .Assign(true)
                .Comment(nameof(SARALYON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Salary[services].IsSalaryEnabled = true;
            return DummyAwaitable.Instance;
        }
    }
}