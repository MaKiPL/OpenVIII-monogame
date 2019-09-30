using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace OpenVIII
{
    public class IGMData_Commands : IGMData
    {
        #region Fields

        private bool _crisisLevel;
        private Kernel_bin.Battle_Commands[] commands;
        private int nonbattleWidth;
        private sbyte page = 0;
        private bool skipReinit;
        private static int s_cidoff = 0;
        private BattleMenu.Mode mode;

        #endregion Fields

        #region Properties

        private int Item_Pool => Count - 2;

        private int Limit_Arrow => Count - 5;

        private int Blue_Pool => Count - 4;

        private int Mag_Pool => Count - 3;

        private int Targets_Window => Count - 1;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Things fixed at startup.
        /// </summary>
        protected override void Init()
        {
            BLANKS[Limit_Arrow] = true;
            base.Init();
            ITEM[Blue_Pool, 0] = new IGMData_BlueMagic_Pool(new Rectangle(X + 50, Y - 20, 300, 192), Character, VisibleCharacter, true);
            ITEM[Blue_Pool, 0].Hide();
            ITEM[Mag_Pool, 0] = new IGMData_Mag_Pool(new Rectangle(X + 50, Y - 20, 300, 192), Character, VisibleCharacter, true);
            ITEM[Mag_Pool, 0].Hide();
            ITEM[Item_Pool, 0] = new IGMData_ItemPool(new Rectangle(X + 50, Y - 22, 400, 194), true);
            ITEM[Item_Pool, 0].Hide();
            ITEM[Targets_Window, 0] = new BattleMenus.IGMData_TargetGroup(Character, VisibleCharacter);
            commands = new Kernel_bin.Battle_Commands[Rows];
            PointerZIndex = Limit_Arrow;
        }

        //public override void Reset()
        //{
        //    ITEM[Targets_Window, 0].HideChildren();
        //    ITEM[Mag_Pool, 0].HideChildren();
        //    ITEM[Item_Pool, 0].HideChildren();
        //    ITEM[Targets_Window, 0].Hide();
        //    ITEM[Mag_Pool, 0].Hide();
        //    ITEM[Item_Pool, 0].Hide();
        //    base.Reset();
        //}
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
                mode = (BattleMenu.Mode)e;
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

        #region Constructors

        public IGMData_Commands(Rectangle pos, Characters character = Characters.Blank, Characters? Visiblecharacter = null, bool battle = false) : base(9, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.COMMAND), 1, 4, character, Visiblecharacter)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        #endregion Constructors

        public bool Battle { get; }

        public bool CrisisLevel { get => _crisisLevel; set => _crisisLevel = value; }

        public IGMData_ItemPool ItemPool => (IGMData_ItemPool)(((IGMData)ITEM[Item_Pool, 0]));
        public IGMData_Mag_Pool MagPool => (IGMData_Mag_Pool)(((IGMData)ITEM[Mag_Pool, 0]));
        public BattleMenus.IGMData_TargetGroup Target_Group => (BattleMenus.IGMData_TargetGroup)(((IGMData)ITEM[Targets_Window, 0]));

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Mag_Pool, 0, ref ret))
            { }
            else if (InputITEM(Item_Pool, 0, ref ret))
            { }
            else if (InputITEM(Targets_Window, 0, ref ret))
            { }
            else if (InputITEM(Blue_Pool, 0, ref ret))
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

        public override bool Inputs_OKAY()
        {
            base.Inputs_OKAY();
            Kernel_bin.Battle_Commands c = commands[CURSOR_SELECT];
            Target_Group.SelectTargetWindows(c);
            switch (c.ID)
            {
                default:
                    // ITEM[Targets_Window, 0].Show();
                    throw new ArgumentOutOfRangeException($"{this}::Command ({c.Name}, {c.ID}) doesn't have explict operation defined!");
                case 1: //ATTACK
                case 14: //SHOT
                case 5: //Renzokuken
                case 12: //MUG
                case 6: //DRAW
                case 29: //CARD
                case 32: //ABSORB
                case 28: //DARKSIDE
                case 30: //DOOM
                case 26: //RECOVER
                case 27: //REVIVE
                case 33: //LVL DOWN
                case 34: //LVL UP
                case 31: //Kamikaze
                case 38: //MiniMog
                case 24: //MADRUSH
                case 25: //Treatment
                case 20: //Desperado
                case 21: //Blood Pain
                case 22: //Massive Anchor
                case 7: //DEVOUR
                case 11: //DUEL
                case 17: //FIRE CROSS / NO MERCY
                case 18: //SORCERY /  ICE STRIKE
                    Target_Group.ShowTargetWindows();
                    return true;
                case 16: // SLOT
                    //TODO add slot menu to randomly choose spell to cast.
                    return true;
                case 19: //COMBINE (ANGELO or ANGEL WING)
                    //TODO see if ANGEL WING unlock if so show menu to choose angelo or angel wing.
                    Target_Group.ShowTargetWindows();
                    return true;
                case 0: //null
                case 9: //CAST is part of DRAW menu.
                case 10: // Stock is part of DRAW menu.
                case 8: // NOMSG
                case 13: // NOMSG
                    return false;
                case 36: //DOUBLE
                case 37: //TRIPLE
                    //TODO 2 casts 2 targets
                    //TODO 3 casts 3 targets
                    ITEM[Mag_Pool, 0].Show();
                    ITEM[Mag_Pool, 0].Refresh();
                    return true;
                case 2: //magic
                    //TODO ADD a menu for DOUBLE and TRIPLE to choose SINGLE DOUBLE OR TRIPLE
                case 35: //SINGLE
                    ITEM[Mag_Pool, 0].Show();
                    ITEM[Mag_Pool, 0].Refresh();
                    return true;
                case 3: //GF
                    //ITEM[GF_Pool, 0].Show();
                    //ITEM[GF_Pool, 0].Refresh();
                    return true;
                case 4: //items
                    ITEM[Item_Pool, 0].Show();
                    ITEM[Item_Pool, 0].Refresh();
                    return true;

                case 15: //Blue magic
                    ITEM[Blue_Pool, 0].Show();
                    ITEM[Blue_Pool, 0].Refresh();
                    return true;
                case 23: //Defend
                    Debug.WriteLine($"{Memory.Strings.GetName(VisibleCharacter)} is using {c.Name}({c.ID})");
                    Menu.BattleMenus.EndTurn();
                    return true;
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
        static int Cidoff
        {
            get => s_cidoff; set
            {
                if (value >= Kernel_bin.BattleCommands.Count)
                    value = 0;
                else if (value < 0)
                    value = Kernel_bin.BattleCommands.Count - 1;
                s_cidoff = value;
            }
        }
        /// <summary>
        /// Things that may of changed before screen loads or junction is changed.
        /// </summary>
        public override void Refresh()
        {
            if (Battle && !mode.Equals(BattleMenu.Mode.YourTurn))
                return;
            if (Memory.State.Characters != null && !skipReinit)
            {
                Rectangle DataSize = Rectangle.Empty;
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
                        if (!Kernel_bin.Commandabilities.TryGetValue(cmd, out Kernel_bin.Command_abilities cmdval))
                        {
                            continue;
                        }
#if DEBUG
                        commands[pos] = Kernel_bin.BattleCommands[Cidoff++];
#else
                        commands[pos] = cmdval.BattleCommand;
#endif
                        ITEM[pos, 0] = new IGMDataItem_String(
                            commands[pos].Name,
                            SIZE[pos]);
                        ITEM[pos, 0].Show();
                        CheckBounds(ref DataSize, pos);
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
                AutoAdjustContainerWidth(DataSize);
                if (Character != Characters.Blank)
                {
                    Target_Group.Refresh(Character, VisibleCharacter);
                    MagPool.Refresh(Character, VisibleCharacter);
                    ItemPool.Refresh(Character, VisibleCharacter);
                }
            }
            skipReinit = false;
        }

        

        public override void Refresh(Characters character, Characters? Visiblecharacter = null)
        {
            base.Refresh(character, Visiblecharacter);
            Target_Group.Refresh(Character, VisibleCharacter);
            MagPool.Refresh(Character, VisibleCharacter);
            ItemPool.Refresh(Character, VisibleCharacter);
        }

        public override void SetModeChangeEvent(ref EventHandler<Enum> eventHandler)
        {
            base.SetModeChangeEvent(ref eventHandler);
            (((IGMData)ITEM[Item_Pool, 0])).SetModeChangeEvent(ref eventHandler);
            (((IGMData)ITEM[Mag_Pool, 0])).SetModeChangeEvent(ref eventHandler);
        }
    }
}