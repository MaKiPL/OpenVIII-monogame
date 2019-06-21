using System;


namespace OpenVIII
{
    /// <summary>
    /// Resets all variables and game data.
    /// Only used when starting a new game (and in debug rooms). 
    /// </summary>
    internal sealed class CLEAR : JsmInstruction
    {
        public CLEAR()
        {
        }

        public CLEAR(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(CLEAR)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Method(nameof(IGameplayService.ResetAllData))
                .Comment(nameof(CLEAR));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].ResetAllData();
            return DummyAwaitable.Instance;
        }
    }
}