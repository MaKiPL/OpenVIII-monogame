using System;


namespace FF8
{
    internal sealed class ADDPARTY : JsmInstruction
    {
        private CharacterId _characterId;

        public ADDPARTY(CharacterId characterId)
        {
            _characterId = characterId;
        }

        public ADDPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (CharacterId)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDPARTY)}({nameof(_characterId)}: {_characterId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.AddPartyCharacter))
                .Enum(_characterId)
                .Comment(nameof(ADDPARTY));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].AddPartyCharacter(_characterId);
            return DummyAwaitable.Instance;
        }
    }
}