using System;


namespace OpenVIII.Fields
{
    internal sealed class SUBPARTY : JsmInstruction
    {
        private IJsmExpression _characterId;

        public SUBPARTY(IJsmExpression characterId)
        {
            _characterId = characterId;
        }

        public SUBPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SUBPARTY)}({nameof(_characterId)}: {_characterId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.RemovePartyCharacter))
                .Enum<Characters>(_characterId)
                .Comment(nameof(SUBPARTY));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Characters characterId = (Characters)_characterId.Int32(services);
            ServiceId.Party[services].RemovePartyCharacter(characterId);
            return DummyAwaitable.Instance;
        }
    }
}
