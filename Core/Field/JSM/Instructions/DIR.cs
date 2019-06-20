using System;


namespace OpenVIII
{
    /// <summary>
    /// Make this entity face "Angle" in degrees. 
    /// </summary>
    internal sealed class DIR : JsmInstruction
    {
        private IJsmExpression _angle;

        public DIR(IJsmExpression angle)
        {
            _angle = angle;
        }

        public DIR(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                angle: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(DIR)}({nameof(_angle)}: {_angle})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetDirection))
                .Argument("angle256", _angle)
                .Comment(nameof(SET3));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetDirection(Degrees.FromAngle256(_angle.Int32(services)));
            return DummyAwaitable.Instance;
        }
    }
}