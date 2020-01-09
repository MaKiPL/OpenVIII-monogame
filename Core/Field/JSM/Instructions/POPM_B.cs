using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Global[index] = (Byte)value;
    /// </summary>
    internal sealed class POPM_B : JsmInstruction
    {
        private GlobalVariableId<Byte> _globalVariable;
        private IJsmExpression _value;

        public POPM_B(GlobalVariableId<Byte> globalVariable, IJsmExpression value)
        {
            _globalVariable = globalVariable;
            _value = value;
        }

        public POPM_B(Int32 parameter, IStack<IJsmExpression> stack)
            : this(new GlobalVariableId<Byte>(parameter),
                value: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(POPM_B)}({nameof(_globalVariable)}: {_globalVariable}, {nameof(_value)}: {_value})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            FormatHelper.FormatGlobalSet(_globalVariable, _value, Jsm.GlobalUInt8, sw, formatterContext, services);
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Byte value = (Byte)_value.Calculate(services);
            ServiceId.Global[services].Set(_globalVariable, value);
            return DummyAwaitable.Instance;
        }
    }
}