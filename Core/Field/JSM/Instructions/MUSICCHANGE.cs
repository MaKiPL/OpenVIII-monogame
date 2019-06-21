using System;


namespace OpenVIII
{
    internal sealed class MUSICCHANGE : JsmInstruction
    {
        public MUSICCHANGE()
        {
        }

        public MUSICCHANGE(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MUSICCHANGE)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IMusicService))
                .Method(nameof(IMusicService.PlayFieldMusic))
                .Comment(nameof(MUSICCHANGE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Music[services].PlayFieldMusic();
            return DummyAwaitable.Instance;
        }
    }
}