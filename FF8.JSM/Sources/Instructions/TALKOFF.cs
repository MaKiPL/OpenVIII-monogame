using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    internal sealed class TALKOFF : JsmInstruction
    {
        public TALKOFF()
        {
        }

        public TALKOFF(Int32 parameter, IStack<IJsmExpression> stack)
            : this()
        {
        }

        public override String ToString()
        {
            return $"{nameof(TALKOFF)}()";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .Property(nameof(FieldObject.Model))
                .Property(nameof(FieldObjectInteraction.IsTalkScriptActive))
                .Assign(false)
                .Comment(nameof(TALKOFF));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            FieldObject currentObject = ServiceId.Field[services].Engine.CurrentObject;
            currentObject.Interaction.IsTalkScriptActive = false;
            return DummyAwaitable.Instance;
        }
    }
}