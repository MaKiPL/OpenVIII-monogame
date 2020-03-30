namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Add party member to active party
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/086_ADDPARTY"/>
    public sealed class ADDPARTY : JsmInstruction
    {
        #region Fields

        /// <summary>
        /// Character ID
        /// </summary>
        private readonly Characters _characterId;

        #endregion Fields

        #region Constructors

        public ADDPARTY(Characters characterId) => _characterId = characterId;

        public ADDPARTY(int parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (Characters)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.AddPartyCharacter))
                .Enum(_characterId)
                .Comment(nameof(ADDPARTY));

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].AddPartyCharacter(_characterId);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(ADDPARTY)}({nameof(_characterId)}: {_characterId})";

        #endregion Methods
    }
}