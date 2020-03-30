namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class MENUNAME : JsmInstruction
    {
        #region Fields

        private readonly IJsmExpression _entityName;

        #endregion Fields

        #region Constructors

        public MENUNAME(IJsmExpression entityName) => _entityName = entityName;

        public MENUNAME(int parameter, IStack<IJsmExpression> stack)
            : this(
                entityName: stack.Pop())
        {
        }

        #endregion Constructors

        #region Methods

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services) => sw.Format(formatterContext, services)
                .Await()
                .StaticType(nameof(IMenuService))
                .Method(nameof(IMenuService.ShowEnterNameDialog))
                .Argument("entityName", _entityName)
                .Comment(nameof(MENUNAME));

        public override IAwaitable TestExecute(IServices services)
        {
            var targetEntity = (NamedEntity)_entityName.Int32(services);
            return ServiceId.Menu[services].ShowEnterNameDialog(targetEntity);
        }

        public override string ToString() => $"{nameof(MENUNAME)}({nameof(_entityName)}: {_entityName})";

        #endregion Methods
    }
}