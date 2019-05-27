using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Enable random battles. 
    /// </summary>
    internal sealed class BATTLEON : JsmInstruction
    {
        public BATTLEON()
        {
        }

        public BATTLEON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(BATTLEON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Property(nameof(IGameplayService.IsRandomBattlesEnabled))
                .Assign(true)
                .Comment(nameof(BATTLEOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].IsRandomBattlesEnabled = true;
            return DummyAwaitable.Instance;
        }
    }
}