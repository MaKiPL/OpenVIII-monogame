using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII.IGMData
{
    public class Commands : Base, IDisposable
    {
        #region Fields

        private bool disposedValue = false;
        private bool EventAdded;

        private Kernel.BattleCommand[] commands;
        private Debug_battleDat.Abilities[] enemycommands;
        private int nonbattleWidth;
        private sbyte page = 0;
        private bool skipRefresh;
        private static int s_cidoff = 0;

        #endregion Fields

        #region Properties

        //Selphie_Slots
        private Selphie_Slots Selphie_Slots { get => (Selphie_Slots)ITEM[Offsets.Selphie_Slots, 0]; set => ITEM[Offsets.Selphie_Slots, 0] = value; }

        private Pool.GF GFPool { get => (Pool.GF)ITEM[Offsets.GF_Pool, 0]; set => ITEM[Offsets.GF_Pool, 0] = value; }
        private Pool.Item ItemPool { get => (Pool.Item)ITEM[Offsets.Item_Pool, 0]; set => ITEM[Offsets.Item_Pool, 0] = value; }
        private Pool.Magic MagPool { get => (Pool.Magic)ITEM[Offsets.Mag_Pool, 0]; set => ITEM[Offsets.Mag_Pool, 0] = value; }
        private Pool.BlueMagic BluePool { get => (Pool.BlueMagic)ITEM[Offsets.Blue_Pool, 0]; set => ITEM[Offsets.Blue_Pool, 0] = value; }
        private Pool.Combine CombinePool { get => (Pool.Combine)ITEM[Offsets.Combine_Pool, 0]; set => ITEM[Offsets.Combine_Pool, 0] = value; }
        private Pool.Bullet BulletPool { get => (Pool.Bullet)ITEM[Offsets.Bullet_Pool, 0]; set => ITEM[Offsets.Bullet_Pool, 0] = value; }
        private IGMDataItem.Icon LimitArrow { get => (IGMDataItem.Icon)ITEM[Offsets.Limit_Arrow, 0]; set => ITEM[Offsets.Limit_Arrow, 0] = value; }

        public IGMData.Target.Group TargetGroup
        {
            get => (IGMData.Target.Group)ITEM[Offsets.Targets_Window, 0];
            protected set => ITEM[Offsets.Targets_Window, 0] = value;
        }

        public IGMData.Pool.EnemyAttacks EnemyAttacks
        {
            get => ((IGMData.Pool.EnemyAttacks)ITEM[Offsets.Enemy_Attacks_Pool, 0]);
            protected set => ITEM[Offsets.Enemy_Attacks_Pool, 0] = value;
        }

        private static class Offsets
        {
            public const int
                Limit_Arrow = 4,
                Start = 5,
                Blue_Pool = Start,
                Mag_Pool = 6,
                GF_Pool = 7,
                Enemy_Attacks_Pool = 8,
                Item_Pool = 9,
                Targets_Window = 10,
                Combine_Pool = 11,
                Selphie_Slots = 12,
                Bullet_Pool = 13,
                Count = 14;
        }

        //base.Inputs_CANCEL();
        private static int Cidoff
        {
            get => s_cidoff; set
            {
                if (value >= Memory.Kernel_Bin.BattleCommands.Count)
                    value = 0;
                else if (value < 0)
                    value = Memory.Kernel_Bin.BattleCommands.Count - 1;
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

            r.SetDamageable(damageable, null);
            r.Init(Offsets.Count, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.COMMAND }, 1, 4);
            return r;
        }

        public override bool Inputs()
        {
            bool ret = false;
            bool found = false;
            //loop through Start to Count
            //This is to only check for input from the dialogs that popup.
            for (int i = Offsets.Start; i < Offsets.Count; i++)
            {
                if (InputITEM(ITEM[i, 0], ref ret))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
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
            if (Battle && CURSOR_SELECT == 0 && CrisisLevel > -1)
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
            Kernel.BattleCommand c = commands[CURSOR_SELECT];
            if (c == null) return false;
            base.Inputs_OKAY();
            TargetGroup.SelectTargetWindows(c);
            if (c.BattleID == 1 && Damageable != null && Damageable.GetEnemy(out Enemy e))
            {
                IEnumerable<Debug_battleDat.Abilities> ecattacks = e.Abilities.Where(x => x.MONSTER != null);
                if (ecattacks.Count() == 1)
                {
                    Kernel.EnemyAttacksData monster = ecattacks.First().MONSTER;
                    TargetGroup.SelectTargetWindows(monster);
                    TargetGroup.ShowTargetWindows();
                }
                else
                {
                    EnemyAttacks.Refresh();
                    EnemyAttacks.Show();
                }

                return true;
            }
            else
                switch (c.BattleID)
                {
                    default:
                        // ITEM[Targets_Window, 0].Show();
                        throw new ArgumentOutOfRangeException($"{this}::Command ({c.Name}, {c.BattleID}) doesn't have explict operation defined!");
                    case 1: //ATTACK
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
                        TargetGroup.ShowTargetWindows();
                        return true;

                    case 16: // SLOT
                             //TODO add slot menu to randomly choose spell to cast.
                        Selphie_Slots.Show();
                        Selphie_Slots.Refresh();
                        return true;

                    case 14: //SHOT
                        BulletPool.Show();
                        BulletPool.Refresh();
                        return true;

                    case 19: //COMBINE (ANGELO or ANGEL WING)
                             //TODO see if ANGEL WING unlock if so show menu to choose angelo or angel wing.
                             //TargetGroup.ShowTargetWindows();
                        CombinePool.Show();
                        CombinePool.Refresh();
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
                        MagPool.Show();
                        MagPool.Refresh();
                        return true;

                    case 2: //magic
                            //TODO ADD a menu for DOUBLE and TRIPLE to choose SINGLE DOUBLE OR TRIPLE
                    case 35: //SINGLE
                        MagPool.Show();
                        MagPool.Refresh();
                        return true;

                    case 3: //GF
                        GFPool.Show();
                        GFPool.Refresh();
                        return true;

                    case 4: //items
                        ItemPool.Show();
                        ItemPool.Refresh();
                        return true;

                    case 15: //Blue magic
                        BluePool.Show();
                        BluePool.Refresh();
                        return true;

                    case 23: //Defend
                        Debug.WriteLine($"{Damageable.Name} is using {c.Name}({c.BattleID})");
                        Damageable.EndTurn();
                        return true;
                }
        }

        public override void Inputs_Right()
        {
            if (Battle && CURSOR_SELECT == 0 && CrisisLevel > -1)
            {
                if (page == 0 && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    commands[CURSOR_SELECT] = c.CharacterStats.Limit;
                    ((IGMDataItem.Text)ITEM[0, 0]).Data = commands[CURSOR_SELECT].Name;
                    skipsnd = true;
                    base.Inputs_Right();
                    page++;
                    LimitArrow.Hide();
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
                if (!skipRefresh)
                {
                    if (Battle)
                    {
                        if ((Damageable.IsGameOver || !Damageable.GetBattleMode().Equals(Damageable.BattleMode.YourTurn)))
                        {
                            Hide();
                            goto end;
                        }
                        else
                            Show();
                    }
                    if (Damageable.GetEnemy(out Enemy e))
                    {
                        enemycommands = e.Abilities;
                        int pos = 0;
                        bool Item, Magic, Attack;
                        Item = Magic = Attack = false;
                        foreach (Debug_battleDat.Abilities a in enemycommands)
                        {
                            if (pos >= Rows) break;
                            ((IGMDataItem.Text)ITEM[pos, 0]).Hide();
                            BLANKS[pos] = true;
                            if (a.ITEM != null)
                            {
                                Item = true;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Data = a.ITEM.Value.Name;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Show();
                                //BLANKS[pos] = false;
                            }
                            else if (a.MAGIC != null)
                            {
                                Magic = true;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Data = a.MAGIC.Name;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Show();
                                //BLANKS[pos] = false;
                            }
                            else if (a.MONSTER != null)
                            {
                                Attack = true;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Data = a.MONSTER.Name;
                                //((IGMDataItem.Text)ITEM[pos, 0]).Show();
                                //BLANKS[pos] = false;
                            }

                            //((IGMDataItem.Text)ITEM[pos, 0]).Pos = SIZE[pos];
                            //pos++;
                        }

                        if (Attack)
                        {
                            IEnumerable<Debug_battleDat.Abilities> ecattacks = enemycommands.Where(x => x.MONSTER != null);
                            AddCommand(Memory.Kernel_Bin.BattleCommands[1], (ecattacks.Count() == 1 ? ecattacks.First().MONSTER.Name : null));
                        }
                        if (Magic || e.DrawList.Any(x => x.DATA != null))
                            AddCommand(Memory.Kernel_Bin.BattleCommands[2]);
                        if (Item || e.DropList.Any(x => x.DATA?.Battle != null) || e.MugList.Any(x => x.DATA?.Battle != null))
                            AddCommand(Memory.Kernel_Bin.BattleCommands[4]);
                        if (e.JunctionedGFs?.Any() ?? false)
                            AddCommand(Memory.Kernel_Bin.BattleCommands[3]);
                        void AddCommand(Kernel.BattleCommand c, FF8String alt = null)
                        {
                            commands[pos] = c;
                            ((IGMDataItem.Text)ITEM[pos, 0]).Data = alt ?? c.Name;
                            ((IGMDataItem.Text)ITEM[pos, 0]).Pos = SIZE[pos];
                            ITEM[pos, 0].Show();
                            BLANKS[pos] = false;
                            pos++;
                        }
                        for (; pos < Rows; pos++)
                        {
                            ITEM[pos, 0].Hide();
                            BLANKS[pos] = true;
                        }
                    }
                    else if (Memory.State.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                    {
                        if (Battle)
                            c.GenerateCrisisLevel();
                        Rectangle DataSize = Rectangle.Empty;
                        page = 0;
                        Cursor_Status &= ~Cursor_Status.Horizontal;
                        commands[0] = Memory.Kernel_Bin.BattleCommands[(c.Abilities.Contains(Kernel.Abilities.Mug) ? 12 : 1)];
                        ITEM[0, 0] = new IGMDataItem.Text
                        {
                            Data = commands[0].Name,
                            Pos = SIZE[0]
                        };

                        for (int pos = 1; pos < Rows; pos++)
                        {
                            Kernel.Abilities cmd = c.Commands[pos - 1];

                            if (cmd != Kernel.Abilities.None)
                            {
                                if (!Memory.Kernel_Bin.CommandAbilities.TryGetValue(cmd, out Kernel.CommandAbilities cmdval))
                                {
                                    continue;
                                }
#if DEBUG
                                if (!Battle) commands[pos] = cmdval.BattleCommand;
                                else commands[pos] = Memory.Kernel_Bin.BattleCommands[Cidoff++];
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
                        if (Battle && CrisisLevel > -1)
                        {
                            CONTAINER.Width = crisiswidth;
                            LimitArrow.Show();
                        }
                        else
                        {
                            CONTAINER.Width = nonbattleWidth;
                            LimitArrow.Hide();
                        }
                        AutoAdjustContainerWidth(DataSize);
                    }
                }
            end:
                skipRefresh = false;
                base.Refresh();
            }
        }

        public override void Refresh(Damageable damageable)
        {
            if (Damageable != damageable)
                skipRefresh = false;
            base.Refresh(damageable);
        }

        /// <summary>
        /// Things fixed at startup.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            BLANKS[Offsets.Limit_Arrow] = true;
            for (int pos = 0; pos < Rows; pos++)
                ITEM[pos, 0] = new IGMDataItem.Text
                {
                    Pos = SIZE[pos]
                };
            GFPool = Pool.GF.Create(new Rectangle(X + 50, Y - 20, 320, 192), Damageable, true);
            GFPool.Hide();
            BluePool = Pool.BlueMagic.Create(new Rectangle(X + 50, Y - 20, 300, 192), Damageable, true);
            BluePool.Hide();
            MagPool = Pool.Magic.Create(new Rectangle(X + 50, Y - 20, 300, 192), Damageable, true);
            MagPool.Hide();
            ItemPool = Pool.Item.Create(new Rectangle(X + 50, Y - 22, 400, 194), Damageable, true);
            ItemPool.Hide();
            EnemyAttacks = Pool.EnemyAttacks.Create(new Rectangle(X + 50, Y - 22, 400, 194), Damageable, true);
            EnemyAttacks.Hide();
            CombinePool = Pool.Combine.Create(new Rectangle(X + 50, Y - 22, 300, 112), Damageable, true);
            CombinePool.Hide();
            BulletPool = Pool.Bullet.Create(new Rectangle(X + 50, Y - 22, 400, 168), Damageable, true);
            BulletPool.Hide();
            Selphie_Slots = Selphie_Slots.Create(new Rectangle(X + 50, Y - 22, 300, 168), Damageable, true);
            Selphie_Slots.Hide();
            LimitArrow = new IGMDataItem.Icon { Data = Icons.ID.Arrow_Right, Pos = new Rectangle(SIZE[0].X + Width - 55, SIZE[0].Y, 0, 0), Palette = 2, Faded_Palette = 7, Blink = true };
            LimitArrow.Hide();
            TargetGroup = Target.Group.Create(Damageable);
            TargetGroup.Hide();
            commands = new Kernel.BattleCommand[Rows];
            enemycommands = null;
            PointerZIndex = Offsets.Limit_Arrow;
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

        public sbyte CrisisLevel => Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c) ? c.CurrentCrisisLevel : (sbyte)-1;

        private void SubscribeEvents()
        {
            if (Damageable != null && !EventAdded)
            {
                Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler += ItemPool.ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler += MagPool.ModeChangeEvent;
                EventAdded = true;
            }
        }

        private void UnsubscribeEvents()
        {
            if (Damageable != null && EventAdded)
            {
                Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler -= ItemPool.ModeChangeEvent;
                Damageable.BattleModeChangeEventHandler -= MagPool.ModeChangeEvent;
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
