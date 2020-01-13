using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class PHSPOWER : JsmInstruction
    {
        private Boolean _isPartySwitchEnabled;

        public PHSPOWER(Boolean isPartySwitchEnabled)
        {
            _isPartySwitchEnabled = isPartySwitchEnabled;
        }

        public PHSPOWER(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                isPartySwitchEnabled: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PHSPOWER)}({nameof(_isPartySwitchEnabled)}: {_isPartySwitchEnabled})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Property(nameof(IPartyService.IsPartySwitchEnabled))
                .Assign(_isPartySwitchEnabled)
                .Comment(nameof(PHSPOWER));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].IsPartySwitchEnabled = _isPartySwitchEnabled;
            return DummyAwaitable.Instance;
        }
    }
}