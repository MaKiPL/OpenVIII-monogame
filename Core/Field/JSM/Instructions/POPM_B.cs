namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Global[index] = (Byte)value;
    /// <para>Pop to memory (byte)</para>
    /// <para>Pop value from stack and store the first byte in Argument.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00B_POPM_B"/>
    public sealed class POPM_B : JsmInstruction
    {
        #region Fields

        private readonly GlobalVariableId<byte> _globalVariable;
        private readonly IJsmExpression _value;

        #endregion Fields

        #region Constructors

        public POPM_B(GlobalVariableId<byte> globalVariable, IJsmExpression value)
        {
            _globalVariable = globalVariable;
            _value = value;
        }

        public POPM_B(int parameter, IStack<IJsmExpression> stack)
            : this(new GlobalVariableId<byte>(parameter),
                value: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalSet(_globalVariable, _value, Jsm.GlobalUInt8, sw, formatterContext, services);

        public override IAwaitable TestExecute(IServices services)
        {
            var value = (byte)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(POPM_B)}({nameof(_globalVariable)}: {_globalVariable}, {nameof(_value)}: {_value})";

        #endregion Methods
    }
}