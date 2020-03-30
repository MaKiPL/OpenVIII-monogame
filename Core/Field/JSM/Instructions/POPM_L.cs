namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Global[index] = (UInt32)value;
    /// <para>Pop to memory (long)</para>
    /// <para>Pop value from stack and store the first four bytes (long) in Argument.</para>
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/00F_POPM_L"/>
    public sealed class POPM_L : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Memory address.
        /// </summary>
        private readonly GlobalVariableId<uint> _globalVariable;

        private readonly IJsmExpression _value;

        #endregion Fields

        #region Constructors

        public POPM_L(GlobalVariableId<uint> globalVariable, IJsmExpression value)
        {
            _globalVariable = globalVariable;
            _value = value;
        }

        public POPM_L(int parameter, IStack<IJsmExpression> stack)
            : this(new GlobalVariableId<uint>(parameter),
                value: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => FormatHelper.FormatGlobalSet(_globalVariable, _value, Jsm.GlobalUInt32, sw, formatterContext, services);

        public override IAwaitable TestExecute(IServices services)
        {
            var value = (uint)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(POPM_L)}({nameof(_globalVariable)}: {_globalVariable}, {nameof(_value)}: {_value})";

        #endregion Methods
    }
}