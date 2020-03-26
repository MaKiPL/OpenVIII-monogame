using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SHOW : JsmInstruction
    {
        public SHOW()
        {
        }

        public SHOW(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(SHOW)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Show))
                .Comment(nameof(SHOW));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.Show();
            return DummyAwaitable.Instance;
        }
    }
}