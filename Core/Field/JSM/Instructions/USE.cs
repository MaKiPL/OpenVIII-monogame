using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Opposite of UNUSE.
    /// </summary>
    internal sealed class USE : JsmInstruction
    {
        public USE()
        {
        }

        public USE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(USE)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.IsActive))
                .Assign(true)
                .Comment(nameof(USE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.IsActive = true;
            return DummyAwaitable.Instance;
        }
    }
}