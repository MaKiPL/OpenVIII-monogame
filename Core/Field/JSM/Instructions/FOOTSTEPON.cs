namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FOOTSTEPON : JsmInstruction
    {
        #region Constructors

        public FOOTSTEPON()
        {
        }

        public FOOTSTEPON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.SoundFootsteps))
                .Assign(true)
                .Comment(nameof(FOOTSTEPON));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.SoundFootsteps = true;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(FOOTSTEPON)}()";

        #endregion Methods
    }
}