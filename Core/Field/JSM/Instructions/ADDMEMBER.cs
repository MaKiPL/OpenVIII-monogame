using System;


namespace OpenVIII
{
    /// <summary>
    /// Adds a PC to the available party (not to the active party). 
    /// </summary>
    internal sealed class ADDMEMBER : JsmInstruction
    {
        private readonly CharacterId _characterId;

        public ADDMEMBER(CharacterId characterId)
        {
            _characterId = characterId;
        }

        public ADDMEMBER(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (CharacterId)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(ADDMEMBER)}({nameof(_characterId)}: {_characterId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.AddPlayableCharacter))
                .Enum(_characterId)
                .Comment(nameof(ADDMEMBER));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].AddPlayableCharacter(_characterId);
            return DummyAwaitable.Instance;
        }
    }
}