using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    /// <summary>
    /// </summary>
    /// <see cref="https://www.youtube.com/watch?v=BhgixAEvuu0"/>
    public class IGMData_BlueMagic_Pool : IGMData_Pool<Saves.Data, Kernel_bin.Blue_Magic>
    {
        public IGMData_BlueMagic_Pool(int count, int depth, IGMDataItem container = null, int? rows = null, int? pages = null, Characters character = Characters.Blank, Characters? visablecharacter = null) : base(count, depth, container, rows, pages, character, visablecharacter)
        {
        }
    }

    public class IGMData_Commands : IGMData
    {
        #region Fields

        private bool _crisisLevel;
        private int nonbattleWidth;
        private sbyte page = 0;
        private bool skipReinit;
        private Kernel_bin.Battle_Commands[] commands;

        #endregion Fields

        #region Constructors

        public IGMData_Commands(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(8, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.COMMAND), 1, 4, character, visablecharacter)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        #endregion Constructors

        #region Properties

        public bool Battle { get; }

        public bool CrisisLevel { get => _crisisLevel; set => _crisisLevel = value; }

        private int Limit_Arrow => Count - 4;

        private int Mag_Pool => Count - 3;

        private int Item_Pool => Count - 2;

        private int Targets_Window => Count - 1;

        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMDataItem_IGMData)ITEM[Targets_Window, 0]).Data);

        #endregion Properties

        #region Methods
        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Mag_Pool, 0, ref ret))
            { }
            else if (InputITEM(Item_Pool, 0, ref ret))
            { }
            else if (InputITEM(Targets_Window, 0, ref ret))
            { }
            else
            {
                Cursor_Status |= Cursor_Status.Enabled;
                Cursor_Status &= ~Cursor_Status.Blinking;
                ret = base.Inputs();
            }
            return ret;
        }

        public override bool Inputs_CANCEL() => false;//base.Inputs_CANCEL();

        public override void Inputs_Left()
        {
            if (Battle && CURSOR_SELECT == 0 && CrisisLevel)
            {
                if (page == 1)
                {
                    Refresh();
                    skipsnd = true;
                    base.Inputs_Left();
                }
            }
        }

        public override void Inputs_Right()
        {
            if (Battle && CURSOR_SELECT == 0 && CrisisLevel)
            {
                if (page == 0)
                {
                    commands[CURSOR_SELECT] = Memory.State.Characters[Character].CharacterStats.Limit;
                    ((IGMDataItem_String)ITEM[0, 0]).Data = commands[CURSOR_SELECT].Name;
                    skipsnd = true;
                    base.Inputs_Right();
                    page++;
                    ITEM[Limit_Arrow, 0].Hide();
                }
            }
        }

        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            Kernel_bin.Battle_Commands c = commands[CURSOR_SELECT];
            Target_Group.SelectTargetWindows(c);
            switch (c.ID)
            {
                default:
                    Target_Group.ShowTargetWindows();
                    return true;
                case 0: //null
                case 7: //nomsg
                    return false;
                case 2: //magic
                    ITEM[Mag_Pool, 0].Show();
                    ITEM[Mag_Pool, 0].Refresh();
                    return true;
                case 4: //items
                    ITEM[Item_Pool, 0].Show();
                    ITEM[Item_Pool, 0].Refresh();
                    return true;
                case 15: //Blue magic
                    ITEM[Mag_Pool, 0].Show();
                    ITEM[Mag_Pool, 0].Refresh();
                    return true;
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
                commands[0] = Kernel_bin.BattleCommands[(Memory.State.Characters[Character].Abilities.Contains(Kernel_bin.Abilities.Mug) ? 12 : 1)];
                ITEM[0, 0] = new IGMDataItem_String(
                        commands[0].Name,
                        SIZE[0]);

                for (int pos = 1; pos < Rows; pos++)
                {
                    Kernel_bin.Abilities cmd = Memory.State.Characters[Character].Commands[pos - 1];

                    if (cmd != Kernel_bin.Abilities.None)
                    {
                        commands[pos] = Kernel_bin.Commandabilities[Memory.State.Characters[Character].Commands[pos - 1]].BattleCommand;
                        ITEM[pos, 0] = new IGMDataItem_String(
                            commands[pos].Name,
                            SIZE[pos]);
                        ITEM[pos, 0].Show();
                        BLANKS[pos] = false;
                    }
                    else
                    {
                        ITEM[pos, 0]?.Hide();
                        BLANKS[pos] = true;
                    }
                }
                const int crisiswidth = 294;
                if (Width != crisiswidth)
                    nonbattleWidth = Width;
                if (Battle && CrisisLevel)
                {
                    CONTAINER.Width = crisiswidth;
                    ITEM[Limit_Arrow, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right, new Rectangle(SIZE[0].X + Width - 55, SIZE[0].Y, 0, 0), 2, 7) { Blink = true };
                }
                else
                {
                    CONTAINER.Width = nonbattleWidth;
                    ITEM[Limit_Arrow, 0] = null;
                }
            }
            skipReinit = false;
        }

        public override void SetModeChangeEvent(ref EventHandler<Enum> eventHandler)
        {
            base.SetModeChangeEvent(ref eventHandler);
            (((IGMDataItem_IGMData)ITEM[Item_Pool, 0]).Data).SetModeChangeEvent(ref eventHandler);
            (((IGMDataItem_IGMData)ITEM[Mag_Pool, 0]).Data).SetModeChangeEvent(ref eventHandler);
        }

        /// <summary>
        /// Things fixed at startup.
        /// </summary>
        protected override void Init()
        {
            BLANKS[Limit_Arrow] = true;
            base.Init();
            ITEM[Mag_Pool, 0] = new IGMDataItem_IGMData(new IGMData_Mag_Pool(new Rectangle(X + 50, Y - 20, 300, 192), Character, VisableCharacter, true));
            ITEM[Mag_Pool, 0].Hide();
            ITEM[Item_Pool, 0] = new IGMDataItem_IGMData(new IGMData_ItemPool(new Rectangle(X + 50, Y - 22, 400, 194), true));
            ITEM[Item_Pool, 0].Hide();
            ITEM[Targets_Window, 0] = new IGMDataItem_IGMData(
                    new BattleMenus.IGMData_TargetGroup());
            commands = new Kernel_bin.Battle_Commands[Rows];
            PointerZIndex = Limit_Arrow;
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
                    CrisisLevel = Memory.State.Characters[Character].GenerateCrisisLevel() >= 0;
                    Show();
                    Refresh();
                }
                else Hide();
            }
        }

        #endregion Methods
    }
}