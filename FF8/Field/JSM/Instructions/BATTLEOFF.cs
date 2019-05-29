using System;


namespace FF8
{
    /// <summary>
    /// Disable random battles. 
    /// </summary>
    internal sealed class BATTLEOFF : JsmInstruction
    {
        public BATTLEOFF()
        {
        }

        public BATTLEOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BATTLEOFF)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Property(nameof(IGameplayService.IsRandomBattlesEnabled))
                .Assign(false)
                .Comment(nameof(BATTLEOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].IsRandomBattlesEnabled = false;
            return DummyAwaitable.Instance;
        }
    }
}