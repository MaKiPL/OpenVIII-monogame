using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class CTURNR : JsmInstruction
    {
        private IJsmExpression _angle;
        private IJsmExpression _frameDuration;

        public CTURNR(IJsmExpression angle, IJsmExpression frameDuration)
        {
            _angle = angle;
            _frameDuration = frameDuration;
        }

        public CTURNR(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameDuration: stack.Pop(),
                angle: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(CTURNR)}({nameof(_angle)}: {_angle}, {nameof(_frameDuration)}: {_frameDuration})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Rotate))
                .Argument("angle", _angle)
                .Argument("frameDuration", _frameDuration)
                .Comment(nameof(CTURNR));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;

            Degrees degrees = Degrees.FromAngle256(_angle.Int32(services));
            Int32 frameDuration = _frameDuration.Int32(services);
            currentObject.Model.Rotate(degrees, frameDuration);

            return DummyAwaitable.Instance;
        }
    }
}