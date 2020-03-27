namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Make this entity face "Angle" in degrees.
    /// </summary>
    internal sealed class DIR : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _angle;

        #endregion Fields

        #region Constructors

        public DIR(IJsmExpression angle) => _angle = angle;

        public DIR(int parameter, IStack<IJsmExpression> stack)
            : this(
                angle: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Method(nameof(FieldObjectModel.SetDirection))
                .Argument("angle256", _angle)
                .Comment(nameof(SET3));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Model.SetDirection(Degrees.FromAngle256(_angle.Int32(services)));
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(DIR)}({nameof(_angle)}: {_angle})";

        #endregion Methods
    }
}