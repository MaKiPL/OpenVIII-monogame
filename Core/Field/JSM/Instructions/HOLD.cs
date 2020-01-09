using System;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Sets whether the PC character can be moved/selected in the switch menu. 
    /// </summary>
    internal sealed class HOLD : JsmInstruction
    {
        private IJsmExpression _characterId;
        private Boolean _isSwitchable;
        private Boolean _isSelectable;

        public HOLD(IJsmExpression characterId, Boolean isSwitchable, Boolean isSelectable)
        {
            _characterId = characterId;
            _isSwitchable = isSwitchable;
            _isSelectable = isSelectable;
        }

        public HOLD(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                isSelectable: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean(),
                isSwitchable: ((Jsm.Expression.PSHN_L)stack.Pop()).Boolean(),
                characterId: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(HOLD)}({nameof(_characterId)}: {_characterId}, {nameof(_isSwitchable)}: {_isSwitchable}, {nameof(_isSelectable)}: {_isSelectable})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.ChangeCharacterState))
                .Enum<Characters>(_characterId)
                .Argument("isSwitchable", _isSwitchable)
                .Argument("isSelectable", _isSelectable)
                .Comment(nameof(HOLD));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            Characters characterId = (Characters)_characterId.Int32(services);
            ServiceId.Party[services].ChangeCharacterState(characterId, _isSwitchable, _isSelectable);
            return DummyAwaitable.Instance;
        }
    }
}