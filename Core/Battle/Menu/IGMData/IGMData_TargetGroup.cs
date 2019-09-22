using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        #region Classes

        public class IGMData_TargetGroup : IGMData_Group
        {
            #region Fields

            private readonly int[] Renzokuken_hits = { 4, 5, 6, 7 };
            private Dictionary<int, Func<bool>> CommandFunc;

            #endregion Fields

            #region Constructors

            //public IGMData_TargetGroup(params IGMData[] d) : base(d) => after();

            public IGMData_TargetGroup(Characters character, Characters? visablecharacter = null, bool makesubs = true)
            {
                VisableCharacter = visablecharacter ?? character;
                const int X = 25;
                const int w = 380;
                const int w2 = 210;
                const int h = 140;
                const int Y1 = 630 - h;
                CONTAINER.Pos = new Rectangle(X, Y1, w + w2, h);
                Init(new IGMData[]{ new IGMData_TargetEnemies(new Rectangle(CONTAINER.Pos.X, CONTAINER.Pos.Y, w, h)),
                    new IGMData_TargetParty(new Rectangle(CONTAINER.Pos.X + w, CONTAINER.Pos.Y, w2, h)),
                    makesubs ? new IGMData_Draw_Pool(new Rectangle(X +50, Y - 50, 300, 192), Character, VisableCharacter, true): null}, true);
                after();
            }
            public override void Refresh(Characters character, Characters? visablecharacter = null)
            {
                base.Refresh(character, visablecharacter);
                Draw_Pool?.Refresh(character, visablecharacter);
            }
            #endregion Constructors

            #region Properties

            public Kernel_bin.Blue_magic_Quistis_limit_break BlueMagic { get; private set; }
            public Kernel_bin.Battle_Commands Command { get; private set; }
            public Item_In_Menu Item { get; private set; }
            public Kernel_bin.Magic_Data Magic { get; private set; }

            private IGMData_Draw_Pool Draw_Pool => (IGMData_Draw_Pool)(((IGMData)ITEM[2, 0]));
            private IGMData_TargetEnemies TargetEnemies => (IGMData_TargetEnemies)(((IGMData)ITEM[0, 0]));

            private IGMData_TargetParty TargetParty => (IGMData_TargetParty)(((IGMData)ITEM[1, 0]));

            #endregion Properties

            #region Methods

            public override bool Inputs()
            {
                bool ret = false;
                if (Draw_Pool?.Enabled ?? false)
                {
                    TargetEnemies.Cursor_Status |= Cursor_Status.Blinking;
                    return Draw_Pool.Inputs();
                }
                else
                {
                    TargetEnemies.Cursor_Status &= ~Cursor_Status.Blinking;
                }
                if (TargetEnemies.Enabled && (((TargetEnemies.Cursor_Status | TargetParty.Cursor_Status) & Cursor_Status.Enabled) == 0 || !TargetParty.Enabled))
                    TargetEnemies.Cursor_Status |= Cursor_Status.Enabled;
                else if (TargetParty.Enabled && (((TargetEnemies.Cursor_Status | TargetParty.Cursor_Status) & Cursor_Status.Enabled) == 0 || !TargetEnemies.Enabled))
                    TargetParty.Cursor_Status |= Cursor_Status.Enabled;

                if (TargetEnemies.Enabled && ((TargetEnemies.Cursor_Status & Cursor_Status.Enabled) != 0 || TargetEnemies.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    TargetEnemies.Cursor_Status |= Cursor_Status.Enabled;
                    TargetParty.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = TargetEnemies.Inputs();
                }
                if (!ret && TargetParty.Enabled && ((TargetParty.Cursor_Status & Cursor_Status.Enabled) != 0 || TargetParty.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    TargetParty.Cursor_Status |= Cursor_Status.Enabled;
                    TargetEnemies.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = TargetParty.Inputs();
                }
                if (!ret)
                {
                    Cursor_Status = Cursor_Status.Hidden | Cursor_Status.Static | Cursor_Status.Enabled;
                    skipdata = true;
                    ret = base.Inputs();
                    skipdata = false;
                }
                return ret;
            }

            public override bool Inputs_CANCEL()
            {
                Hide();
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                if (CommandFunc.TryGetValue(Command.ID, out Func<bool> val))
                    return val();
                else
                    return CommandDefault();
            }

            public void SelectTargetWindows(Item_In_Menu c)
            {
                Kernel_bin.Target t = c.Battle.Target;
                SelectTargetWindows(t);
                Command = Kernel_bin.BattleCommands[4];
                Item = c;
            }

            public void SelectTargetWindows(Kernel_bin.Battle_Commands c)
            {
                Kernel_bin.Target t = c.Target;
                SelectTargetWindows(t);
                Command = c;
                Magic = null;
                BlueMagic = null;
            }

            public void SelectTargetWindows(Kernel_bin.Magic_Data c)
            {
                Kernel_bin.Target t = c.Target;
                SelectTargetWindows(t);
                Command = Kernel_bin.BattleCommands[2];
                Magic = c;
            }

            public void SelectTargetWindows(Kernel_bin.Blue_magic_Quistis_limit_break c)
            {
                //not sure if target data is missing for blue magic.
                //The target box does show up in game so I imagine the target data is in there somewhere.
                Kernel_bin.Target t = c.Target;
                SelectTargetWindows(t);
                Command = Kernel_bin.BattleCommands[15];
                BlueMagic = c;
            }

            public override void Show() => base.Show();

            public void ShowTargetWindows()
            {
                skipdata = true;
                Show();
                skipdata = false;
                Refresh();
            }

            protected override void Init()
            {
                if (CommandFunc == null)
                    CommandFunc = new Dictionary<int, Func<bool>>
                    {
                        {0,Command00 },
                        {1,Command01_ATTACK },
                        {2,Command02_MAGIC },
                        {3,Command03 },
                        {4,Command04_ITEM },
                        {5,Command05_RENZOKUKEN },
                        {6,Command06_DRAW },
                        {7,Command07 },
                        {8,Command08 },
                        {9,Command09 },
                        {10,Command10 },
                        {11,Command11 },
                        {12,Command12 },
                        {13,Command13 },
                        {14,Command14 },
                        {15,Command15 },
                        {16,Command16 },
                        {17,Command17 },
                        {18,Command18 },
                        {19,Command19 },
                        {20,Command20 },
                        {21,Command21 },
                        {22,Command22 },
                        {23,Command23 },
                        {24,Command24 },
                        {25,Command25 },
                        {26,Command26 },
                        {27,Command27 },
                        {28,Command28 },
                        {29,Command29 },
                        {30,Command30 },
                        {31,Command31 },
                        {32,Command32 },
                        {33,Command33 },
                        {34,Command34 },
                        {35,Command35 },
                        {36,Command36 },
                        {37,Command37 },
                        {38,Command38 },
                        {39,Command39 },
                    };
                base.Init();
            }

            private void after()
            {
                TargetEnemies.Target_Party = TargetParty;
                TargetParty.Target_Enemies = TargetEnemies;
                Draw_Pool?.Hide();
                Hide();
            }

            private bool Command00() => throw new NotImplementedException();

            private bool Command01_ATTACK()
            {
                Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget);
                return true;
            }

            private bool Command02_MAGIC()
            {
                Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget);
                Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} casts {Magic.Name}({Magic.ID}) spell on { (enemytarget ? $"{e.Name.Value_str}({TargetEnemies.CURSOR_SELECT})" : Memory.Strings.GetName(vc).Value_str) }");
                return true;
            }

            private bool Command03() => throw new NotImplementedException();

            private bool Command04_ITEM()
            {
                Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget);
                Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Item.Name}({Item.ID}) item on { (enemytarget ? $"{e.Name.Value_str}({TargetEnemies.CURSOR_SELECT})" : Memory.Strings.GetName(vc).Value_str) }");
                return true;
            }

            private bool Command05_RENZOKUKEN()
            {
                Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget);
                //Renzokuken
                byte w = Memory.State[fromvc].WeaponID;
                int hits = Memory.State[fromvc].CurrentCrisisLevel < Renzokuken_hits.Length ? Renzokuken_hits[Memory.State[fromvc].CurrentCrisisLevel] : Renzokuken_hits.Last();
                int finisherchance = (Memory.State[fromvc].CurrentCrisisLevel + 1) * 60;
                bool willfinish = Memory.Random.Next(byte.MaxValue+1) <= finisherchance;
                int choosefinish = Memory.Random.Next(3+1);
                Kernel_bin.Weapons_Data wd = Kernel_bin.WeaponsData[w];
                Kernel_bin.Renzokeken_Finisher r = wd.Renzokuken;
                List<Kernel_bin.Renzokeken_Finisher> flags = Enum.GetValues(typeof(Kernel_bin.Renzokeken_Finisher))
                    .Cast<Kernel_bin.Renzokeken_Finisher>()
                    .Where(f => (f & r) != 0)
                    .ToList();
                Kernel_bin.Renzokeken_Finisher finisher = choosefinish >= flags.Count ? flags.Last() : flags[choosefinish];
                //per wiki the chance of which finisher is 25% each and the highest value finisher get the remaining of 100 percent.
                //so rough divide is 100% when you only only have that
                //when you unlock 2 one is 75% chance
                //when you onlock 3 last one is 50%
                //when you unlock all 4 it's 25% each.

                //finishers each have their own target
                if (willfinish)
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} hits {hits} times with {Command.Name}({Command.ID}) then uses {Kernel_bin.RenzokukenFinishersData[finisher].Name}.");

                if (!willfinish)
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} hits {hits} times with {Command.Name}({Command.ID}) then fails to use a finisher.");
                return true;
            }

            private bool Command06_DRAW()
            {
                Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget);
                //draw
                //spawn a 1 page 4 row pool of the magic/gfs that the selected enemy has.
                DrawMagic(e.DrawList);
                Draw_Pool.Refresh(e.DrawList);
                Draw_Pool.Show();
                return true;
            }

            private bool Command07() => throw new NotImplementedException();

            private bool Command08() => throw new NotImplementedException();

            private bool Command09() => throw new NotImplementedException();

            private bool Command10() => throw new NotImplementedException();

            private bool Command11() => throw new NotImplementedException();

            private bool Command12() => throw new NotImplementedException();

            private bool Command13() => throw new NotImplementedException();

            private bool Command14() => throw new NotImplementedException();

            private bool Command15() => throw new NotImplementedException();

            private bool Command16() => throw new NotImplementedException();

            private bool Command17() => throw new NotImplementedException();

            private bool Command18() => throw new NotImplementedException();

            private bool Command19() => throw new NotImplementedException();

            private bool Command20() => throw new NotImplementedException();

            private bool Command21() => throw new NotImplementedException();

            private bool Command22() => throw new NotImplementedException();

            private bool Command23() => throw new NotImplementedException();

            private bool Command24() => throw new NotImplementedException();

            private bool Command25() => throw new NotImplementedException();

            private bool Command26() => throw new NotImplementedException();

            private bool Command27() => throw new NotImplementedException();

            private bool Command28() => throw new NotImplementedException();

            private bool Command29() => throw new NotImplementedException();

            private bool Command30() => throw new NotImplementedException();

            private bool Command31() => throw new NotImplementedException();

            private bool Command32() => throw new NotImplementedException();

            private bool Command33() => throw new NotImplementedException();

            private bool Command34() => throw new NotImplementedException();

            private bool Command35() => throw new NotImplementedException();

            private bool Command36() => throw new NotImplementedException();

            private bool Command37() => throw new NotImplementedException();

            private bool Command38() => throw new NotImplementedException();

            private bool Command39() => throw new NotImplementedException();

            private bool CommandDefault() => throw new NotImplementedException();

            private void DebugMessageCommand(IGMData_TargetEnemies i, Enemy e, Characters vc, Characters fromvc, bool enemytarget) => Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Command.Name}({Command.ID}) command on { (enemytarget ? $"{e.Name.Value_str}({i.CURSOR_SELECT})" : Memory.Strings.GetName(vc).Value_str) }");

            private void DrawMagic(Debug_battleDat.Magic[] drawList) =>
                                                                                                                                                                                                                                                                                                                                                                                                                                                    // display
                                                                                                                                                                                                                                                                                                                                                                                                                                                    // pool
                                                                                                                                                                                                                                                                                                                                                                                                                                                    // with list.
                                                                                                                                                                                                                                                                                                                                                                                                                                                    Debug.WriteLine($"Display draw pool: {string.Join(", ", drawList)}");

            private void Neededvaribles(out Enemy e, out Characters vc, out Characters fromvc, out bool enemytarget)
            {
                e = Enemy.Party[TargetParty.CURSOR_SELECT < Enemy.Party.Count ? TargetParty.CURSOR_SELECT : Enemy.Party.Count - 1];
                Characters c = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[TargetParty.CURSOR_SELECT];
                vc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[TargetParty.CURSOR_SELECT];
                int p = BattleMenus.Player;
                Characters fromc = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[p];
                fromvc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[p];
                enemytarget = false;
                if ((TargetEnemies.Cursor_Status & Cursor_Status.Enabled) != 0 && TargetEnemies.Enabled)
                    enemytarget = true;
                DebugMessageCommand(TargetEnemies, e, vc, fromvc, enemytarget);
            }

            private void SelectTargetWindows(Kernel_bin.Target t)
            {
                if ((t & Kernel_bin.Target.Ally) != 0)
                    TargetParty.Show();
                else
                    TargetParty.Hide();
                if ((t & Kernel_bin.Target.Enemy) != 0)
                    TargetEnemies.Show();
                else
                    TargetEnemies.Hide();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}