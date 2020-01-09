using System;


namespace OpenVIII.Fields
{
    internal sealed class BGDRAW : JsmInstruction
    {
        private IJsmExpression _arg0;

        public BGDRAW(IJsmExpression arg0)
        {
            _arg0 = arg0;
        }

        public BGDRAW(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg0: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGDRAW)}({nameof(_arg0)}: {_arg0})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.DrawBackground))
                .Comment(nameof(BGDRAW));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Rendering[services].DrawBackground();
            return DummyAwaitable.Instance;
        }
    }
}