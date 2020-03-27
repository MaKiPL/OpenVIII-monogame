namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class SETPC : JsmInstruction
    {
        #region Fields

        private readonly Characters _characterId;

        #endregion Fields

        #region Constructors

        public SETPC(Characters characterId) => _characterId = characterId;

        public SETPC(int parameter, IStack<IJsmExpression> stack)
            : this(
                characterId: (Characters)((Jsm.Expression.PSHN_L)stack.Pop()).Value)
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Method(nameof(FieldObject.BindCharacter))
                .Enum(_characterId)
                .Comment(nameof(SETPC));

        public override IAwaitable TestExecute(IServices services)
        {
            var currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.BindCharacter(_characterId);
            return DummyAwaitable.Instance;
        }

        public override string ToString() => $"{nameof(SETPC)}({nameof(_characterId)}: {_characterId})";

        #endregion Methods
    }
}