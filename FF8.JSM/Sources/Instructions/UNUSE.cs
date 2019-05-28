using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Disable this entity's scripts, hides its model, and makes it throughable. Call USE to re-enable. 
    /// </summary>
    internal sealed class UNUSE : JsmInstruction
    {
        private Int32 _parameter;

        public UNUSE(Int32 parameter)
        {
            _parameter = parameter;
        }

        public UNUSE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        public override String ToString()
        {
            return $"{nameof(UNUSE)}({nameof(_parameter)}: {_parameter})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.IsActive))
                .Assign(false)
                .Comment(nameof(UNUSE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.IsActive = false;
            return DummyAwaitable.Instance;
        }
    }
}