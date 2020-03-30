namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Sets the bounds of this line object (for its touchOn, touchOff, and across scripts).
    /// Lines are actually 3d hitboxes, not lines.
    /// </summary>
    internal sealed class SETLINE : JsmInstruction
    {
        #region Fields

        private readonly Coords3D _p1;
        private readonly Coords3D _p2;

        #endregion Fields

        #region Constructors

        public SETLINE(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            _p1 = new Coords3D(x1, y1, z1);
            _p2 = new Coords3D(x2, y2, z2);
        }

        public SETLINE(int parameter, IStack<IJsmExpression> stack)
            : this(
                z2: ((IConstExpression)stack.Pop()).Int32(),
                y2: ((IConstExpression)stack.Pop()).Int32(),
                x2: ((IConstExpression)stack.Pop()).Int32(),
                z1: ((IConstExpression)stack.Pop()).Int32(),
                y1: ((IConstExpression)stack.Pop()).Int32(),
                x1: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetHitBox))
                .Argument("p1", _p1.ToString())
                .Argument("p2", _p2.ToString())
                .Comment(nameof(SETLINE));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetHitBox(_p1, _p2);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SETLINE)}({nameof(_p1)}: {_p1}, {nameof(_p2)}: {_p2})";

        #endregion Methods
    }
}