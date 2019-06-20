using System;


namespace OpenVIII
{
    internal sealed class MAPFADEON : JsmInstruction
    {
        public MAPFADEON()
        {
        }

        public MAPFADEON(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(MAPFADEON)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.FadeOn))
                .Comment(nameof(MAPFADEON));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].FadeOn();
            return DummyAwaitable.Instance;
        }
    }
}