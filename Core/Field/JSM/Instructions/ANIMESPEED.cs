using System;


namespace FF8
{
    internal sealed class ANIMESPEED : JsmInstruction
    {
        private Int32 _fps;

        public ANIMESPEED(Int32 fps)
        {
            _fps = fps;
        }

        public ANIMESPEED(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                fps: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32() * 2) // Native: FPS / 2
        {
        }

        public override String ToString()
        {
            return $"{nameof(ANIMESPEED)}({nameof(_fps)}: {_fps})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Animation))
                .Property(nameof(FieldObjectAnimation.FPS))
                .Assign(_fps)
                .Comment(nameof(ANIMESPEED));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].Engine.CurrentObject.Animation.FPS = _fps;
            return DummyAwaitable.Instance;
        }
    }
}