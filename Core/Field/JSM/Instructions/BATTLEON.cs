namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Enable random battles.
    /// </summary>
    internal sealed class BATTLEON : JsmInstruction
    {
        #region Constructors

        public BATTLEON()
        {
        }

        public BATTLEON(int parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Property(nameof(IGameplayService.IsRandomBattlesEnabled))
                .Assign(true)
                .Comment(nameof(BATTLEOFF));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].IsRandomBattlesEnabled = true;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(BATTLEON)}()";

        #endregion Methods
    }
}