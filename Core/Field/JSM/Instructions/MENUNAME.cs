using System;


namespace OpenVIII
{
    internal sealed class MENUNAME : JsmInstruction
    {
        private IJsmExpression _entityName;

        public MENUNAME(IJsmExpression entityName)
        {
            _entityName = entityName;
        }

        public MENUNAME(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                entityName: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MENUNAME)}({nameof(_entityName)}: {_entityName})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Await()
                .StaticType(nameof(IMenuService))
                .Method(nameof(IMenuService.ShowEnterNameDialog))
                .Argument("entityName", _entityName)
                .Comment(nameof(MENUNAME));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            NamedEntity targetEntity = (NamedEntity)_entityName.Int32(services);
            return ServiceId.Menu[services].ShowEnterNameDialog(targetEntity);
        }
    }
}