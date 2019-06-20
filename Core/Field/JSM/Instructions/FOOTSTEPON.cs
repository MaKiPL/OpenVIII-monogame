using System;


namespace FF8
{
    internal sealed class FOOTSTEPON : JsmInstruction
    {
        public FOOTSTEPON()
        {
        }

        public FOOTSTEPON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FOOTSTEPON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.SoundFootsteps))
                .Assign(true)
                .Comment(nameof(FOOTSTEPON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.SoundFootsteps = true;
            return DummyAwaitable.Instance;
        }
    }
}