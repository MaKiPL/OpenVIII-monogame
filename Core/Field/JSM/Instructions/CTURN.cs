namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Make this entity face the entity with the ID of the first parameter.
    /// </summary>
    internal sealed class CTURN : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _frameDuration;
        private readonly int _targetObject;

        #endregion Fields

        #region Constructors

        public CTURN(int targetObject, IJsmExpression frameDuration)
        {
            _targetObject = targetObject;
            _frameDuration = frameDuration;
        }

        public CTURN(int parameter, IStack<IJsmExpression> stack)
            : this(
                frameDuration: stack.Pop(),
                targetObject: ((IConstExpression)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Await()
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.RotateToObject))
                .Argument("targetObject", _targetObject)
                .Argument("frameDuration", _frameDuration)
                .Comment(nameof(CTURN));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;

            var frameDuration = _frameDuration.Int32(services);
            currentObject.Model.RotateToObject(_targetObject, frameDuration);

            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(CTURN)}({nameof(_targetObject)}: {_targetObject}, {nameof(_frameDuration)}: {_frameDuration})";

        #endregion Methods
    }
}