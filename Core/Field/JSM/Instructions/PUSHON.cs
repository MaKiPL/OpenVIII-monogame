using System;


namespace OpenVIII
{
    /// <summary>
    /// Enables "pushing" this entity. An entity's "push" script is run when the player walks into it. Detection range can presumably be set with PUSHRADIUS
    /// </summary>
    internal sealed class PUSHON : JsmInstruction
    {
        public PUSHON()
        {
        }

        public PUSHON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(PUSHON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.IsPushScriptActive))
                .Assign(true)
                .Comment(nameof(PUSHON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.IsPushScriptActive = true;
            return DummyAwaitable.Instance;
        }
    }
}