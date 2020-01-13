using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class BGANIMESPEED : JsmInstruction
    {
        private IJsmExpression _halfFps;

        public BGANIMESPEED(IJsmExpression halfFps)
        {
            _halfFps = halfFps;
        }

        public BGANIMESPEED(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                halfFps: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(BGANIMESPEED)}({nameof(_halfFps)}: {_halfFps})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IRenderingService))
                .Property(nameof(IRenderingService.BackgroundFPS))
                .Assign(_halfFps)
                .Comment(nameof(BGANIMESPEED));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Int32 fps = _halfFps.Int32(services) * 2;
            ServiceId.Rendering[services].BackgroundFPS = fps;
            return DummyAwaitable.Instance;
        }
    }
}