namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Temp[index] = value;
    /// <para>Pop to Temp List (long)</para>
    /// <para>Pop the top value from the stack and store the first four bytes (long) at index position Argument in the Temp List.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/009_POPI_L"/>
    public sealed class POPI_L : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Index position in the Temp List (0 &lt;= Argument &lt; 8).
        /// </summary>
        private readonly ScriptResultId _index;

        private readonly IJsmExpression _value;

        #endregion Fields

        #region Constructors

        public POPI_L(ScriptResultId index, IJsmExpression value)
        {
            _index = index;
            _value = value;
        }

        public POPI_L(int parameter, IStack<IJsmExpression> stack)
            : this(new ScriptResultId(parameter),
                value: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Append($"R{_index.ResultId}");
            sw.Append(" = ");
            _value.Format(sw, formatterContext, services);
            sw.AppendLine(";");
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var value = _value.Int32(services);
            ServiceId.Interaction[services][_index] = value;
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(POPI_L)}({nameof(_index)}: {_index}, {nameof(_value)}: {_value})";

        #endregion Methods
    }
}