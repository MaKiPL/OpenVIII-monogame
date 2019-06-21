using System;


namespace OpenVIII
{
    /// <summary>
    /// Enables user control. See UCOFF for details. 
    /// </summary>
    internal sealed class UCON : JsmInstruction
    {
        public UCON()
        {
        }

        public UCON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(UCON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IGameplayService))
                .Property(nameof(IGameplayService.IsUserControlEnabled))
                .Assign(true)
                .Comment(nameof(UCON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Gameplay[services].IsUserControlEnabled = true;
            return DummyAwaitable.Instance;
        }
    }
}