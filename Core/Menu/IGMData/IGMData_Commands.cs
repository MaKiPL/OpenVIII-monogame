using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public class IGMData_Commands : IGMData
    {
        #region Fields

        private int nonbattleWidth;
        private sbyte page = 0;
        private bool skipReinit;

        #endregion Fields

        #region Constructors

        public IGMData_Commands(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.COMMAND), 1, 4, character, visablecharacter)
        {
            Battle = battle;
            nonbattleWidth = Width;
            skipReinit = true;
            Refresh();
        }

        #endregion Constructors

        #region Properties

        public bool Battle { get; }

        #endregion Properties

        #region Methods

        public override bool Inputs()
        {
            Cursor_Status |= Cursor_Status.Enabled;
            return base.Inputs();
        }

        public override bool Inputs_CANCEL() => false;//base.Inputs_CANCEL();

        public override void Inputs_Left()
        {
            if (Battle && Memory.State.Characters[Character].GenerateCrisisLevel() >= 0 && CURSOR_SELECT == 1 || true)
            {
                if (page == 1)
                {
                    Refresh();
                    base.Inputs_Left();
                    //for (int i = 1; i < Count; i++)
                    //{
                    //    ITEM[i, 0].Hide();
                    //    BLANKS[i] = true;
                    //}
                }
            }
        }

        public override void Inputs_Right()
        {
            if (Battle && Memory.State.Characters[Character].GenerateCrisisLevel() >= 0 && CURSOR_SELECT == 1 || true)
            {
                if (page == 0)
                {
                    ((IGMDataItem_String)ITEM[0, 0]).Data = Memory.State.Characters[Character].CharacterStats.Limit.Name;
                    base.Inputs_Right();
                    page++;
                    ITEM[Count - 1, 0].Hide();
                    //for (int i = 1; i < Count; i++)
                    //{
                    //    ITEM[i, 0].Hide();
                    //    BLANKS[i] = true;
                    //}
                }
            }
        }

        /// <summary>
        /// Things that may of changed before screen loads or junction is changed.
        /// </summary>
        public override void Refresh()
        {
            if (Memory.State.Characters != null && !skipReinit)
            {
                base.Refresh();
                page = 0;
                Cursor_Status &= ~Cursor_Status.Horizontal;
                ITEM[0, 0] = new IGMDataItem_String(
                        Kernel_bin.BattleCommands[
                            Memory.State.Characters[Character].Abilities.Contains(Kernel_bin.Abilities.Mug) ?
                            13 :
                            1].Name,
                        SIZE[0]);

                for (int pos = 1; pos < rows; pos++)
                {
                    Kernel_bin.Abilities cmd = Memory.State.Characters[Character].Commands[pos - 1];

                    if (cmd != Kernel_bin.Abilities.None)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(
                            Kernel_bin.Commandabilities[Memory.State.Characters[Character].Commands[pos - 1]].Name,
                            SIZE[pos]);
                        ITEM[pos, 0].Show();
                        BLANKS[pos] = false;
                    }
                    else
                    {
                        ITEM[pos, 0].Hide();
                        BLANKS[pos] = true;
                    }
                }

                if (Battle && Memory.State.Characters[Character].GenerateCrisisLevel() >= 0 || true) //TODO remove true for testing limitbreak
                {
                    CONTAINER.Width = 294;
                    ITEM[Count - 1, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right, new Rectangle(SIZE[0].X + Width - 55, SIZE[0].Y, 0, 0), 2, 7) { Blink = true };
                }
                else
                {
                    CONTAINER.Width = nonbattleWidth;
                    ITEM[Count - 1, 0] = null;
                }
            }
            skipReinit = false;
        }

        /// <summary>
        /// Things fixed at startup.
        /// </summary>
        protected override void Init()
        {
            BLANKS[Count - 1] = true;
            base.Init();
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        protected override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(BattleMenu.Mode))
            {
                BattleMenu.Mode mode = (BattleMenu.Mode)e;
                if (mode.Equals(BattleMenu.Mode.YourTurn))
                {
                    Show();
                    Refresh();
                }
                else Hide();
            }
        }

        #endregion Methods
    }
}