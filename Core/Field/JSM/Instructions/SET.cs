using System;


namespace OpenVIII.Fields
{
    /// <summary>
    /// Place this entity's model at XCoord, YCoord standing on the given walkmesh triangle. Unlike SET3, this function will place the event on the walkable terrain (the ZCoord is interpolated from the walkmesh). 
    /// </summary>
    internal sealed class SET : JsmInstruction
    {
        private Int32 _walkmeshTriangleId;
        private Int32 _x;
        private Int32 _y;

        public SET(Int32 walkmeshTriangleId, Int32 x, Int32 y)
        {
            _walkmeshTriangleId = walkmeshTriangleId;
            _x = x;
            _y = y;
        }

        public SET(Int32 walkmeshTriangleId, IStack<IJsmExpression> stack)
            : this(walkmeshTriangleId,
                y: ((IConstExpression)stack.Pop()).Int32(),
                x: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SET)}({nameof(_walkmeshTriangleId)}: {_walkmeshTriangleId}, {nameof(_x)}: {_x}, {nameof(_y)}: {_y})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetPosition))
                .Argument("walkmeshTriangleId", _walkmeshTriangleId)
                .Argument("x", _x)
                .Argument("y", _y)
                .Comment(nameof(SET3));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetPosition(new WalkmeshCoords(_walkmeshTriangleId, _x, _y));
            return DummyAwaitable.Instance;
        }
    }
}