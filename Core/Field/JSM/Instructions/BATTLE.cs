using System;
using static OpenVIII.Fields.Scripts.Jsm.Expression;

namespace OpenVIII.Fields.Scripts.Instructions
{
    /// <summary>
    /// trigger Battle encounter
    /// </summary>
    /// <see cref="http://wiki.ffrtt.ru/index.php?title=FF8/Field/Script/Opcodes/069_BATTLE"/>
    public sealed class BATTLE : JsmInstruction
    {
        private IJsmExpression _arg0;
        private IJsmExpression _arg1;

        public BATTLE(IJsmExpression arg0, IJsmExpression arg1)
        {
            _arg0 = arg0;
            _arg1 = arg1;
        }

        public BATTLE(Int32 parameter, IStack<IJsmExpression> stack)
            : this(
                arg1: stack.Pop(),
                arg0: stack.Pop())
        {
        }
        public ushort Encounter => checked((ushort)((PSHN_L)_arg0).Value);
        public BFlags Flags => checked((BFlags)((PSHN_L)_arg1).Value);
        [Flags]
        public enum BFlags : byte
        {
            Regular_battle = 0x0,
            No_escape = 0x1,
            /// <summary>
            /// (battle music keeps playing after win/loss)
            /// </summary>
            Disable_victory_fanfare = 0x2,
            Inherit_countdown_timer_from_field = 0x4,
            No_Item_XP_Gain = 0x8,
            Use_current_music_as_battle_music = 0x10,
            Force_preemptive_attacked = 0x20,
            Force_back_attack = 0x40,
            Unknown = 0x80
        }
        public override String ToString()
        {
            return $"{nameof(BATTLE)}({nameof(_arg0)}: {_arg0}, {nameof(_arg1)}: {_arg1})";
        }
    }
}