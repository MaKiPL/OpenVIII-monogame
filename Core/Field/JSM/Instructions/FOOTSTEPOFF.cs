using System;


namespace OpenVIII.Fields
{
    internal sealed class FOOTSTEPOFF : JsmInstruction
    {
        public FOOTSTEPOFF()
        {
        }

        public FOOTSTEPOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPOFF)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.SoundFootsteps))
                .Assign(false)
                .Comment(nameof(FOOTSTEPOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.SoundFootsteps = false;
            return DummyAwaitable.Instance;
        }
    }
}