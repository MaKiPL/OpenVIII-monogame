using System;


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
        /// <summary>
        /// Memory address.
        /// </summary>
        private GlobalVariableId<UInt32> _globalVariable;
        private IJsmExpression _value;

        public POPM_L(GlobalVariableId<UInt32> globalVariable, IJsmExpression value)
        {
            _globalVariable = globalVariable;
            _value = value;
        }

        public POPM_L(Int32 parameter, IStack<IJsmExpression> stack)
            : this(new GlobalVariableId<UInt32>(parameter),
                value: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(POPM_L)}({nameof(_globalVariable)}: {_globalVariable}, {nameof(_value)}: {_value})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            FormatHelper.FormatGlobalSet(_globalVariable, _value, Jsm.GlobalUInt32, sw, formatterContext, services);
        }

        public override IAwaitable TestExecute(IServices services)
        {
            var value = (UInt32)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }
    }
}