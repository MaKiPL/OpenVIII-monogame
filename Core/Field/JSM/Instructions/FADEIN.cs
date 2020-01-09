using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class FADEIN : JsmInstruction
    {
        public FADEIN()
        {
        }

        public FADEIN(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(FADEIN)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.FadeIn))
                .Comment(nameof(FADEIN));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].FadeIn();
            return DummyAwaitable.Instance;
        }
    }
}