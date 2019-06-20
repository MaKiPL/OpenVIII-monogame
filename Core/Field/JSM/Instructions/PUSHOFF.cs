using System;


namespace OpenVIII
{
    /// <summary>
    /// Disables this entity's "push" script. See PUSHON. 
    /// </summary>
    internal sealed class PUSHOFF : JsmInstruction
    {
        public PUSHOFF()
        {
        }

        public PUSHOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(PUSHOFF)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.IsPushScriptActive))
                .Assign(false)
                .Comment(nameof(PUSHOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.IsPushScriptActive = false;
            return DummyAwaitable.Instance;
        }
    }
}