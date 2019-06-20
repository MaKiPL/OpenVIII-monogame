using System;


namespace FF8
{
    internal sealed class SETPC : JsmInstruction
    {
        private CharacterId _characterId;

        public SETPC(CharacterId characterId)
        {
            _characterId = characterId;
        }

        public SETPC(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (CharacterId)((Jsm.Expression.PSHN_L)stack.Pop()).Value)
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETPC)}({nameof(_characterId)}: {_characterId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Method(nameof(FieldObject.BindChracter))
                .Enum(_characterId)
                .Comment(nameof(SETPC));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.BindChracter(_characterId);
            return DummyAwaitable.Instance;
        }
    }
}