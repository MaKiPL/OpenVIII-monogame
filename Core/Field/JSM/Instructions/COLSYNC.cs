using System;
namespace OpenVIII
{
    internal sealed class COLSYNC : JsmInstruction
    {
        public COLSYNC()
        {
        }

        public COLSYNC(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(COLSYNC)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Method(nameof(IRenderingService.Wait))
                .Comment(nameof(COLSYNC));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            return ServiceId.Rendering[services].Wait();
        }
    }
}