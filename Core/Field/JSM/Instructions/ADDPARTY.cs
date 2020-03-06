using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add party member to active party
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/086_ADDPARTY"/>
    public sealed class ADDPARTY : JsmInstruction
    {
        /// <summary>
        /// Character BattleID
        /// </summary>
        private Characters _characterId;

        public ADDPARTY(Characters characterId)
        {
            _characterId = characterId;
        }

        public ADDPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (Characters)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
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