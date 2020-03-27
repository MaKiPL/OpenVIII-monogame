namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Exits the current script and all scripts that are waiting on it. To end only the current script, use RET instead.
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/01C_HALT"/>
    public sealed class HALT : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Always 0.
        /// </summary>
        private readonly int _parameter;

        #endregion Fields

        #region Constructors

        public HALT(int parameter) => _parameter = parameter;

        public HALT(int parameter, IStack<IJsmExpression> stack)
            : this(parameter)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Scripts))
                .Method(nameof(FieldObjectScripts.CancelAll))
                .Comment(nameof(HALT));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Scripts.CancelAll();
            return BreakAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(HALT)}({nameof(_parameter)}: {_parameter})";

        #endregion Methods
    }
}