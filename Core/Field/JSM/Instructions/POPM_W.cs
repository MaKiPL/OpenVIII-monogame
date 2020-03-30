namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Global[index] = (UInt16)value;
    /// <para>Pop to memory (word)</para>
    /// <para>Pop value from stack and store the first two bytes (word) in Argument.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00D_POPM_W"/>
    public sealed class POPM_W : JsmInstruction
    {
        #region Fields

        private readonly GlobalVariableId<ushort> _globalVariable;
        private readonly IJsmExpression _value;

        #endregion Fields

        #region Constructors

        public POPM_W(GlobalVariableId<ushort> globalVariable, IJsmExpression value)
        {
            _globalVariable = globalVariable;
            _value = value;
        }

        public POPM_W(int parameter, IStack<IJsmExpression> stack)
            : this(new GlobalVariableId<ushort>(parameter),
                value: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalSet(_globalVariable, _value, Jsm.GlobalUInt16, sw, formatterContext, services);

        public override IAwaitable TestExecute(IServices services)
        {
            var value = (ushort)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(POPM_W)}({nameof(_globalVariable)}: {_globalVariable}, {nameof(_value)}: {_value})";

        #endregion Methods
    }
}