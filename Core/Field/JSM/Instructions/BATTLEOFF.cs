namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Disable random battles.
    /// </summary>
    internal sealed class BATTLEOFF : JsmInstruction
    {
        #region Constructors

        public BATTLEOFF()
        {
        }

        public BATTLEOFF(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Property(nameof(IGameplayService.IsRandomBattlesEnabled))
                .Assign(false)
                .Comment(nameof(BATTLEOFF));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].IsRandomBattlesEnabled = false;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(BATTLEOFF)}()";

        #endregion Methods
    }
}