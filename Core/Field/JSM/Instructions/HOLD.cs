namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// Sets whether the PC character can be moved/selected in the switch menu.
    /// </summary>
    internal sealed class HOLD : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _characterId;
        private readonly bool _isSelectable;
        private readonly bool _isSwitchable;

        #endregion Fields

        #region Constructors

        public HOLD(IJsmExpression characterId, bool isSwitchable, bool isSelectable)
        {
            _characterId = characterId;
            _isSwitchable = isSwitchable;
            _isSelectable = isSelectable;
        }

        public HOLD(int parameter, IStack<IJsmExpression> stack)
            : this(
                isSelectable: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean(),
                isSwitchable: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean(),
                characterId: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.ChangeCharacterState))
                .Enum<Characters>(_characterId)
                .Argument("isSwitchable", _isSwitchable)
                .Argument("isSelectable", _isSelectable)
                .Comment(nameof(HOLD));

        public override IAwaitable TestExecute(IServices services)
        {
            var characterId = (Characters)_characterId.Int32(services);
            ServiceId.Party[services].ChangeCharacterState(characterId, _isSwitchable, _isSelectable);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(HOLD)}({nameof(_characterId)}: {_characterId}, {nameof(_isSwitchable)}: {_isSwitchable}, {nameof(_isSelectable)}: {_isSelectable})";

        #endregion Methods
    }
}