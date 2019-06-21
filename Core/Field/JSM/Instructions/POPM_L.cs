using System;


namespace OpenVIII
{
    /// <summary>
    /// Global[index] = (UInt32)value;
    /// </summary>
    internal sealed class POPM_L : JsmInstruction
    {
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
            UInt32 value = (UInt32)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }
    }
}