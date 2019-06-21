using System;


namespace OpenVIII
{
    internal sealed class SETPLACE : JsmInstruction
    {
        private readonly Int32 _areaId;

        public SETPLACE(Int32 areaId)
        {
            _areaId = areaId;
        }

        public SETPLACE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                areaId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETPLACE)}({nameof(_areaId)}: {_areaId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .CommentLine(AreaName.Get(_areaId))
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.BindArea))
                .Argument("areaId", _areaId)
                .Comment(nameof(SETPLACE));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].BindArea(_areaId);
            return DummyAwaitable.Instance;
        }
    }
}