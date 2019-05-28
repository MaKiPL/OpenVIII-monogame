using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Jump the player to the field with the given ID and starting on the given walkmesh triangle. The walkmesh is almost always 0 because MAPJUMPO is intended to be used for teleporting the player into a cutscene, and the cutscenes place the characters where they need to be on initialization, so it doesn't matter where they're initially teleported.
    /// </summary>
    internal sealed class MAPJUMPO : JsmInstruction
    {
        private FieldId _fieldMapId;
        private Int32 _walkmeshId;

        public MAPJUMPO(FieldId fieldMapId, Int32 walkmeshId)
        {
            _fieldMapId = fieldMapId;
            _walkmeshId = walkmeshId;
        }

        public MAPJUMPO(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                walkmeshId: ((Jsm.Expression.PSHN_L)stack.Pop()).Int32(),
                fieldMapId: (FieldId)((Jsm.Expression.PSHN_L)stack.Pop()).Int32())
        {
        }

        public override String ToString()
        {
            return $"{nameof(MAPJUMPO)}({nameof(_fieldMapId)}: {_fieldMapId}, {nameof(_walkmeshId)}: {_walkmeshId})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .CommentLine(FieldName.Get(_fieldMapId))
                .StaticType(nameof(IFieldService))
                .Method(nameof(IFieldService.GoTo))
                .Enum(_fieldMapId)
                .Argument("walkmeshId", _walkmeshId)
                .Comment(nameof(MAPJUMPO));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Field[services].GoTo(_fieldMapId, _walkmeshId);
            return DummyAwaitable.Instance;
        }
    }
}