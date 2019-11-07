using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace OpenVIII.IGMData
{
    public class Commands : Base, IDisposable
    {
        #region Fields

        private bool disposedValue = false;
        private bool EventAdded;

        private bool _crisisLevel;
        private Kernel_bin.Battle_Commands[] commands;
        private int nonbattleWidth;
        private sbyte page = 0;
        private bool skipRefresh;
        private static int s_cidoff = 0;

        #endregion Fields

        #region Properties

        public bool Battle { get; set; } = false;
        public bool CrisisLevel { get => _crisisLevel; set => _crisisLevel = value; }
        public IGMData.Pool.Item GFPool => (IGMData.Pool.Item)(((Base)ITEM[GF_Pool, 0]));
        public IGMData.Pool.Item ItemPool => (IGMData.Pool.Item)(((Base)ITEM[Item_Pool, 0]));
        public IGMData.Pool.Magic MagPool => (IGMData.Pool.Magic)(((Base)ITEM[Mag_Pool, 0]));
        public IGMData.Target.Group Target_Group => (IGMData.Target.Group)(((Base)ITEM[Targets_Window, 0]));
        private int Item_Pool => Count - 2;

        private int GF_Pool => Count - 6;
        private int Limit_Arrow => Count - 5;

        private int Blue_Pool => Count - 4;

        private int Mag_Pool => Count - 3;

        private int Targets_Window => Count - 1;

        //base.Inputs_CANCEL();
        private static int Cidoff
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

        #endregion Properties

        #region Methods

        // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
                UnsubscribeEvents();
                GC.SuppressFinalize(this);
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() =>
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);

        public static Commands Create(Rectangle pos, Damageable damageable = null, bool battle = false)
        {
            Commands r = new Commands
            {
                skipRefresh = damageable == null,
                Battle = battle
            };

            r.Init(damageable, null);
            r.Init(10, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.COMMAND }, 1, 4);
            return r;
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputITEM(Mag_Pool, 0, ref ret))
            { }
            else if (InputITEM(Item_Pool, 0, ref ret))
            { }
            else if (InputITEM(GF_Pool, 0, ref ret))
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

        public override bool Inputs_CANCEL() => false;

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
                    ITEM[GF_Pool, 0].Show();
                    ITEM[GF_Pool, 0].Refresh();
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
                    Debug.WriteLine($"{Damageable.Name} is using {c.Name}({c.ID})");
                    Damageable.EndTurn();
                    return true;
            }
        }

        public override void Inputs_Right()
        {
            if (Battle && CURSOR_SELECT == 0 && CrisisLevel)
            {
                if (page == 0 && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    commands[CURSOR_SELECT] = c.CharacterStats.Limit;
                    ((IGMDataItem.Text)ITEM[0, 0]).Data = commands[CURSOR_SELECT].Name;
                    skipsnd = true;
                    base.Inputs_Right();
                    page++;
                    ITEM[Limit_Arrow, 0].Hide();
                }
            }
        }

        /// <summary>
        /// Things that may of changed before screen loads or junction is changed.
        /// </summary>
        public override void Refresh()
        {
            if (Damageable != null)
            {
                if (Memory.State.Characters != null && !skipRefresh && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    if (Battle && (Damageable.IsGameOver || !Damageable.GetBattleMode().Equals(Damageable.BattleMode.YourTurn)))
                    {
                        Hide();
                        return;
                    }
                    else
                        Show();
                    if (Battle)
                        CrisisLevel = c.GenerateCrisisLevel() >= 0;
                    Rectangle DataSize = Rectangle.Empty;
                    base.Refresh();
                    page = 0;
                    Cursor_Status &= ~Cursor_Status.Horizontal;
                    commands[0] = Kernel_bin.BattleCommands[(c.Abilities.Contains(Kernel_bin.Abilities.Mug) ? 12 : 1)];
                    ITEM[0, 0] = new IGMDataItem.Text
                    {
                        Data = commands[0].Name,
                        Pos = SIZE[0]
                    };

                    for (int pos = 1; pos < Rows; pos++)
                    {
                        Kernel_bin.Abilities cmd = c.Commands[pos - 1];

                        if (cmd != Kernel_bin.Abilities.None)
                        {
                            if (!Kernel_bin.Commandabilities.TryGetValue(cmd, out Kernel_bin.Command_abilities cmdval))
                            {
                                continue;
                            }
#if DEBUG
                            if (!Battle) commands[pos] = cmdval.BattleCommand;
                            else commands[pos] = Kernel_bin.BattleCommands[Cidoff++];
#else
                        commands[pos] = cmdval.BattleCommand;
#endif
                            ((IGMDataItem.Text)ITEM[pos, 0]).Data = commands[pos].Name;
                            ((IGMDataItem.Text)ITEM[pos, 0]).Pos = SIZE[pos];
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
                    if (Battle && CrisisLevel)
                    {
                        CONTAINER.Width = crisiswidth;
                        ITEM[Limit_Arrow, 0].Show();
                    }
                    else
                    {
                        CONTAINER.Width = nonbattleWidth;
                        ITEM[Limit_Arrow, 0].Hide();
                    }
                    AutoAdjustContainerWidth(DataSize);
                    if (Damageable != null)
                    {
                        Target_Group.Refresh(Damageable);
                        MagPool.Refresh(Damageable);
                        ItemPool.Refresh(Damageable);
                    }
                }
                skipRefresh = false;
            }
        }

        public override void Refresh(Damageable damageable)
        {
            if (Damageable != damageable)
                skipRefresh = false;
            base.Refresh(damageable);
            Target_Group.Refresh(Damageable);
            MagPool.Refresh(Damageable);
            ItemPool.Refresh(Damageable);
        }

        /// <summary>
        /// Things fixed at startup.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            BLANKS[Limit_Arrow] = true;
            for (int pos = 0; pos < Rows; pos++)
                ITEM[pos, 0] = new IGMDataItem.Text
                {
                    Pos = SIZE[pos]
                };
            ITEM[GF_Pool, 0] = Pool.GF.Create(new Rectangle(X + 50, Y - 20, 320, 192), Damageable, true);
            ITEM[GF_Pool, 0].Hide();
            ITEM[Blue_Pool, 0] = Pool.BlueMagic.Create(new Rectangle(X + 50, Y - 20, 300, 192), Damageable, true);
            ITEM[Blue_Pool, 0].Hide();
            ITEM[Mag_Pool, 0] = Pool.Magic.Create(new Rectangle(X + 50, Y - 20, 300, 192), Damageable, true);
            ITEM[Mag_Pool, 0].Hide();
            ITEM[Item_Pool, 0] = Pool.Item.Create(new Rectangle(X + 50, Y - 22, 400, 194), Damageable, true);
            ITEM[Item_Pool, 0].Hide();
            ITEM[Limit_Arrow, 0] = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Right, Pos = new Rectangle(SIZE[0].X + Width - 55, SIZE[0].Y, 0, 0), Palette = 2, Faded_Palette = 7, Blink = true };
            ITEM[Limit_Arrow, 0].Hide();
            ITEM[Targets_Window, 0] = IGMData.Target.Group.Create(Damageable);
            commands = new Kernel_bin.Battle_Commands[Rows];
            PointerZIndex = Limit_Arrow;
            nonbattleWidth = Width;
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-22, -8);
            SIZE[i].Offset(0, 12 + (-8 * row));
        }

        private Damageable.BattleMode BattleMode = Damageable.BattleMode.EndTurn;

        public override void ModeChangeEvent(object sender, Enum e)
        {
            base.ModeChangeEvent(sender, e);
            if (e.GetType() == typeof(Damageable.BattleMode) && !BattleMode.Equals(e))
            {
                BattleMode = (Damageable.BattleMode)e;
                Refresh();
            }
        }

        public override Damageable Damageable
        {
            get => base.Damageable;
            protected set
            {
                if (value != base.Damageable)
                {
                    if (Battle)
                    {
                        UnsubscribeEvents();
                        base.Damageable = value;
                        SubscribeEvents();
                    }
                    else
                        base.Damageable = value;
                }
            }
        }

        private void SubscribeEvents()
        {
            if (Damageable != null && !EventAdded)
            {
                Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler += (((Base)ITEM[Item_Pool, 0])).ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler += (((Base)ITEM[Mag_Pool, 0])).ModeChangeEvent;
                EventAdded = true;
            }
        }

        private void UnsubscribeEvents()
        {
            if (Damageable != null && EventAdded)
            {
                Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler -= (((Base)ITEM[Item_Pool, 0])).ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler -= (((Base)ITEM[Mag_Pool, 0])).ModeChangeEvent;
                EventAdded = false;
            }
        }

        #endregion Methods

        ~Commands()
        {
            Dispose(false);
        }
    }
}