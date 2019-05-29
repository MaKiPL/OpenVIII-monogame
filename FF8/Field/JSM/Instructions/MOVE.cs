using System;


namespace FF8
{
    /// <summary>
    /// Make this entity walk/run to the given coordinates. 
    /// </summary>
    internal sealed class MOVE : JsmInstruction
    {
        private IJsmExpression _x;
        private IJsmExpression _y;
        private IJsmExpression _z;
        private IJsmExpression _unknown;

        public MOVE(IJsmExpression x, IJsmExpression y, IJsmExpression z, IJsmExpression unknown)
        {
            _x = x;
            _y = y;
            _z = z;
            _unknown = unknown;
        }

        public MOVE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                unknown: stack.Pop(),
                z: stack.Pop(),
                y: stack.Pop(),
                x: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MOVE)}({nameof(_x)}: {_x}, {nameof(_y)}: {_y}, {nameof(_z)}: {_z}, {nameof(_unknown)}: {_unknown})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectInteraction.Move))
                .Argument("x", _x)
                .Argument("y", _y)
                .Argument("z", _z)
                .Argument("unknown", _unknown)
                .Comment(nameof(MOVE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;

            Coords3D coords = new Coords3D(
                _x.Int32(services),
                _y.Int32(services),
                _z.Int32(services));
            Int32 unknown = _unknown.Int32(services);

            currentObject.Interaction.Move(coords, unknown);

            return DummyAwaitable.Instance;
        }
    }
}