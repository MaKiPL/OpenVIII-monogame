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
        #region Fields

        private readonly ushort _encounter;
        private readonly BFlags _flags;

        #endregion Fields

        #region Constructors

        public BATTLE(IJsmExpression encounter, IJsmExpression flags)
        {
            if (encounter is PSHI_L || encounter is PSHSM_W)
            {
                //requires service to evaluate value?
                _encounter = ushort.MaxValue;//so I set an invalid value here.
            }
            else
                _encounter = ((IConstExpression)encounter).UInt16();
            _flags = (BFlags)((IConstExpression)flags).Int32();
        }

        public BATTLE(int parameter, IStack<IJsmExpression> stack)
            : this(
                flags: stack.Pop(),
                encounter: stack.Pop())
        {
        }

        #endregion Constructors

        #region Enums

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

        #endregion Enums

        #region Properties

        public ushort Encounter => _encounter;
        public BFlags Flags => _flags;

        #endregion Properties

        #region Methods

        public override string ToString() => $"{nameof(BATTLE)}({nameof(_encounter)}: {_encounter}, {nameof(_flags)}: {_flags})";

        #endregion Methods
    }
}