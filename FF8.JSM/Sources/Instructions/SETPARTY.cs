using System;
using FF8.Core;
using FF8.Framework;
using FF8.JSM.Format;

namespace FF8.JSM.Instructions
{
    /// <summary>
    /// Sets the active party to be the members with the input IDs. These IDs also work with the other party related functions.
    /// </summary>
    internal sealed class SETPARTY : JsmInstruction
    {
        private IJsmExpression _character1;
        private IJsmExpression _character2;
        private IJsmExpression _character3;

        public SETPARTY(IJsmExpression character1, IJsmExpression character2, IJsmExpression character3)
        {
            _character1 = character1;
            _character2 = character2;
            _character3 = character3;
        }

        public SETPARTY(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                character3: stack.Pop(),
                character2: stack.Pop(),
                character1: stack.Pop())
        {
        }

        public override String ToString()
        {
            return $"{nameof(SETPARTY)}({nameof(_character1)}: {_character1}, {nameof(_character2)}: {_character2}, {nameof(_character3)}: {_character3})";
        }

        public override void Format(ScriptWriter sw, IScriptFormatterContext formatterContext, IServices services)
        {
            sw.Format(formatterContext, services)
                .StaticType(nameof(IPartyService))
                .Method(nameof(IPartyService.ChangeParty))
                .Enum<CharacterId>(_character1)
                .Enum<CharacterId>(_character2)
                .Enum<CharacterId>(_character3)
                .Comment(nameof(SETPARTY));
        }

        public override IAwaitable TestExecute(IServices services)
        {
            ServiceId.Party[services].ChangeParty(
                (CharacterId)_character1.Int32(services),
                (CharacterId)_character1.Int32(services),
                (CharacterId)_character1.Int32(services));
            return DummyAwaitable.Instance;
        }
    }
}