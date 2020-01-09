using System;


namespace OpenVIII.Fields.Scripts.Instructions
{
    internal sealed class PREMAPJUMP2 : JsmInstruction
    {
        private readonly int _fieldMapId;

        public PREMAPJUMP2(int fieldMapId)
        {
            _fieldMapId = fieldMapId;
        }

        public PREMAPJUMP2(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                fieldMapId: (int)((IConstExpression)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(PREMAPJUMP2)}({nameof(_fieldMapId)}: {_fieldMapId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .CommentLine(FieldName.Get(_fieldMapId))
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.PrepareGoTo))
                .Enum(_fieldMapId)
                .Comment(nameof(PREMAPJUMP2));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].PrepareGoTo(_fieldMapId);
            return DummyAwaitable.Instance;
        }
    }
}