using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Sets the bounds of this line object (for its touchOn, touchOff, and across scripts).
    /// Lines are actually 3d hitboxes, not lines. 
    /// </summary>
    internal sealed class SETLINE : JsmInstruction
    {
        private readonly Coords3D _p1;
        private readonly Coords3D _p2;

        public SETLINE(Int32 x1, Int32 y1, Int32 z1, Int32 x2, Int32 y2, Int32 z2)
        {
            _p1 = new Coords3D(x1, y1, z1);
            _p2 = new Coords3D(x2, y2, z2);
        }

        public SETLINE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                z2: ((IConstExpression)stack.Pop()).Int32(),
                y2: ((IConstExpression)stack.Pop()).Int32(),
                x2: ((IConstExpression)stack.Pop()).Int32(),
                z1: ((IConstExpression)stack.Pop()).Int32(),
                y1: ((IConstExpression)stack.Pop()).Int32(),
                x1: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETLINE)}({nameof(_p1)}: {_p1}, {nameof(_p2)}: {_p2})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetHitBox))
                .Argument("p1", _p1.ToString())
                .Argument("p2", _p2.ToString())
                .Comment(nameof(SETLINE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetHitBox(_p1, _p2);
            return DummyAwaitable.Instance;
        }
    }
}