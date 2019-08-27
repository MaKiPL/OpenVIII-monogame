using System;


namespace OpenVIII
{
    /// <summary>
    /// Removes a PC from the available party and the active party. 
    /// </summary>
    internal sealed class SUBMEMBER : JsmInstruction
    {
        private readonly Characters _characterId;

        public SUBMEMBER(Characters characterId)
        {
            _characterId = characterId;
        }

        public SUBMEMBER(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (Characters)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SUBMEMBER)}({nameof(_characterId)}: {_characterId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.RemovePlayableCharacter))
                .Enum(_characterId)
                .Comment(nameof(SUBMEMBER));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].RemovePlayableCharacter(_characterId);
            return DummyAwaitable.Instance;
        }
    }
}