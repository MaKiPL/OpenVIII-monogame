using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Place this entity's model at XCoord, YCoord, ZCoord standing on the given walkmesh triangle. 
    /// </summary>
    internal sealed class SET3 : JsmInstruction
    {
        private Int32 _walkmeshTriangleId;
        private Int32 _x;
        private Int32 _y;
        private Int32 _z;

        public SET3(Int32 walkmeshTriangleId, Int32 x, Int32 y, Int32 z)
        {
            _walkmeshTriangleId = walkmeshTriangleId;
            _x = x;
            _y = y;
            _z = z;
        }

        public SET3(Int32 walkmeshTriangleId, IStack<IJsmExpression> stack)
            : this(walkmeshTriangleId,
                z: ((IConstExpression)stack.Pop()).Int32(),
                y: ((IConstExpression)stack.Pop()).Int32(),
                x: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SET3)}({nameof(_walkmeshTriangleId)}: {_walkmeshTriangleId}, {nameof(_x)}: {_x}, {nameof(_y)}: {_y}, {nameof(_z)}: {_z})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetPosition))
                .Argument("walkmeshTriangleId", _walkmeshTriangleId)
                .Argument("x", _x)
                .Argument("y", _y)
                .Argument("z", _z)
                .Comment(nameof(SET3));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetPosition(new WalkmeshCoords(_walkmeshTriangleId, _x, _y, _z));
            return DummyAwaitable.Instance;
        }
    }
}