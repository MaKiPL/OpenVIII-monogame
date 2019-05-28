using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Turns this entity. 
    /// </summary>
    internal sealed class CTURNL : JsmInstruction
    {
        private IJsmExpression _angle;
        private IJsmExpression _frameDuration;

        public CTURNL(IJsmExpression angle, IJsmExpression frameDuration)
        {
            _angle = angle;
            _frameDuration = frameDuration;
        }

        public CTURNL(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                frameDuration: stack.Pop(),
                angle: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(CTURNL)}({nameof(_angle)}: {_angle}, {nameof(_frameDuration)}: {_frameDuration})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.Rotate))
                .Argument("angle", _angle)
                .Argument("frameDuration", _frameDuration)
                .Comment(nameof(CTURNL));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;

            Degrees degrees = Degrees.FromAngle256(_angle.Int32(services));
            Int32 frameDuration = _frameDuration.Int32(services);
            currentObject.Model.Rotate(-degrees, frameDuration);

            return DummyAwaitable.Instance;
        }
    }
}