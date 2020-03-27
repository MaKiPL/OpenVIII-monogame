namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// <para>Unuse entity</para>
    /// <para>Disable this entity's scripts, hides its model, and makes it throughable. Call USE to re-enable. </para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01A_UNUSE"/>
    public sealed class UNUSE : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Always 0.
        /// </summary>
        private readonly int _parameter;

        #endregion Fields

        #region Constructors

        public UNUSE(int parameter) => _parameter = parameter;

        public UNUSE(int parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.IsActive))
                .Assign(false)
                .Comment(nameof(UNUSE));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.IsActive = false;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(UNUSE)}({nameof(_parameter)}: {_parameter})";

        #endregion Methods
    }
}