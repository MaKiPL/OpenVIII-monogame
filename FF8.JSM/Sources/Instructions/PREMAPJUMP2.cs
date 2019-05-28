using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    internal sealed class PREMAPJUMP2 : JsmInstruction
    {
        private readonly FieldId _fieldMapId;

        public PREMAPJUMP2(FieldId fieldMapId)
        {
            _fieldMapId = fieldMapId;
        }

        public PREMAPJUMP2(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                fieldMapId: (FieldId)((IConstExpression)stack.Pop()).Int32())
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