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

            //public IGMData_TargetGroup(params IGMData[] d) : base(d) => after();

            #region Properties

            //private IGMData_BlueMagic_Pool BlueMagic_Pool => (IGMData_BlueMagic_Pool)(((IGMData)ITEM[3, 0]));
            private IGMData_Draw_Pool Draw_Pool => (IGMData_Draw_Pool)(((IGMData)ITEM[2, 0]));

            private IGMData_TargetEnemies TargetEnemies => (IGMData_TargetEnemies)(((IGMData)ITEM[0, 0]));

            private IGMData_TargetParty TargetParty => (IGMData_TargetParty)(((IGMData)ITEM[1, 0]));

            #endregion Properties

            #region Methods

            private void after()
            {
                TargetEnemies.Target_Party = TargetParty;
                TargetParty.Target_Enemies = TargetEnemies;
                Draw_Pool?.Hide();
                Hide();
            }



            private bool CommandDefault() => throw new NotImplementedException();

            private void DebugMessageCommand(IGMData_TargetEnemies i, Damageable[] d, Characters fromvc) => Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Command.Name}({Command.ID}) command on { DebugMessageSuffix(d) }");

            private string DebugMessageSuffix(Damageable[] d) => $"{DamageableNames(d)}({(d.Length == 1 ? TargetEnemies.CURSOR_SELECT.ToString() : "MultiSelect")})";

            /// <summary>
            /// Display pool with list
            /// </summary>
            /// <param name="drawList"></param>
            private void DrawMagic(Debug_battleDat.Magic[] drawList) => Debug.WriteLine($"Display draw pool: {string.Join(", ", drawList)}");

            private string DamageableNames(Damageable[] e)
            {
                string r = $"[{e[0].Name}";
                for (int j = 1; j < e.Length; j++)
                    r += $", {e[j]}";
                r += "]";
                return r;
            }

            private void Neededvaribles(out Damageable[] d, out Characters fromvc)
            {
                Damageable[] e;
                Damageable[] vc;
                if ((Target & Kernel_bin.Target.Single_Target) != 0)
                {
                    e = new Enemy[] { Enemy.Party[TargetParty.CURSOR_SELECT < Enemy.Party.Count ? TargetParty.CURSOR_SELECT : Enemy.Party.Count - 1] };
                    var charvar = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[TargetParty.CURSOR_SELECT];
                    vc = new Saves.CharacterData[] { Memory.State[charvar] };
                }
                else
                {
                    vc = Memory.State.Party.Where(x => x != Characters.Blank).Select(y=>Memory.State[y]).ToArray();
                    e = Enemy.Party.ToArray();
                }
                Characters c = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[TargetParty.CURSOR_SELECT];
                int p = BattleMenus.Player;
                Characters fromc = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[p];
                fromvc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[p];
                d = vc;
                if ((TargetEnemies.Cursor_Status & Cursor_Status.Enabled) != 0 && TargetEnemies.Enabled)
                    d = e;
                DebugMessageCommand(TargetEnemies, d, fromvc);
            }

            private void SelectTargetWindows(Kernel_bin.Target t)
            {
                Target = t;
                if ((t & Kernel_bin.Target.Ally) != 0 || t == Kernel_bin.Target.None || ((t & Kernel_bin.Target.Enemy) == 0 && (t & Kernel_bin.Target.Single_Side) != 0))
                {
                    TargetParty.Show();
                    TargetAll(TargetParty);
                }
                else
                    TargetParty.Hide();
                if ((t & Kernel_bin.Target.Enemy) != 0)
                {
                    TargetEnemies.Show();
                    TargetAll(TargetEnemies);
                }
                else
                    TargetEnemies.Hide();
                void TargetAll(IGMData i)
                {
                    if ((Target & Kernel_bin.Target.Single_Target) == 0)
                        i.Cursor_Status |= Cursor_Status.All;
                    else
                        i.Cursor_Status &= ~Cursor_Status.All;
                }
            }

            protected override void Init()
            {
                if (CommandFunc == null)
                    CommandFunc = new Dictionary<int, Func<bool>>
                    {
                        //{0,Command00 },
                        {1,Command01_ATTACK },
                        {2,Command02_MAGIC },
                        //{3,Command03_GF },
                        {4,Command04_ITEM },
                        {5,Command05_RENZOKUKEN },
                        {6,Command06_DRAW },
                        {7,Command07_DEVOUR },
                        //{8,Command08_UNNAMED },
                        //{9,Command09_CAST },
                        //{10,Command10_STOCK },
                        {11,Command11_DUEL },
                        {12,Command12_MUG },
                        //{13,Command13_NOMSG },
                        {14,Command14_SHOT },
                        {15,Command15_BLUE_MAGIC },
                        //{16,Command16_SLOT },
                        {17,Command17_FIRE_CROSS_NO_MERCY },
                        {18,Command18_SORCERY_ICE_STRIKE },
                        {19,Command19_COMBINE },
                        {20,Command20_DESPERADO },
                        {21,Command21_BLOOD_PAIN },
                        {22,Command22_MASSIVE_ANCHOR },
                        //{23,Command23_DEFEND },
                        {24,Command24_MADRUSH },
                        {25,Command25_TREATMENT },
                        {26,Command26_RECOVERY },
                        {27,Command27_REVIVE },
                        {28,Command28_DARKSIDE },
                        {29,Command29_CARD },
                        {30,Command30_DOOM },
                        {31,Command31_KAMIKAZI },
                        {32,Command32_ABSORB },
                        {33,Command33_LVL_DOWN },
                        {34,Command34_LVL_UP },
                        {35,Command35_SINGLE },
                        {36,Command36_DOUBLE },
                        {37,Command37_TRIPLE },
                        {38,Command38_MINIMOG },
                    };
                base.Init();

                //bool Command00() => throw new NotImplementedException();

                bool Command01_ATTACK()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Damageable.EndTurn();
                    return true;
                }

                bool Command02_MAGIC()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} casts {Magic.Name}({Magic.ID}) spell on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();
                    return true;
                }

                //bool Command03_GF() => throw new NotImplementedException();

                bool Command04_ITEM()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Item.Name}({Item.ID}) item on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();
                    return true;
                }

                bool Command05_RENZOKUKEN()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    if (d.First().GetType() == typeof(Enemy))
                    {
                        //Renzokuken
                        byte w = Memory.State[fromvc].WeaponID;
                        int hits = 0;
                        if (Memory.State[fromvc].CurrentCrisisLevel > 0)
                            hits = Memory.State[fromvc].CurrentCrisisLevel < Renzokuken_hits.Length ? Renzokuken_hits[Memory.State[fromvc].CurrentCrisisLevel] : Renzokuken_hits.Last();
                        //else return false;
                        else hits = Renzokuken_hits.First();
                        int finisherchance = (Memory.State[fromvc].CurrentCrisisLevel + 1) * 60;
                        bool willfinish = Memory.Random.Next(byte.MaxValue + 1) <= finisherchance;
                        int choosefinish = Memory.Random.Next(3 + 1);
                        Kernel_bin.Weapons_Data wd = Kernel_bin.WeaponsData[w];
                        Kernel_bin.Renzokeken_Finisher r = wd.Renzokuken;
                        if(r == 0)
                            willfinish = false;
                        
                        //per wiki the chance of which finisher is 25% each and the highest value finisher get the remaining of 100 percent.
                        //so rough divide is 100% when you only only have that
                        //when you unlock 2 one is 75% chance
                        //when you onlock 3 last one is 50%
                        //when you unlock all 4 it's 25% each.

                        //finishers each have their own target
                        BattleMenus.GetCurrentBattleMenu().Renzokeken.Reset(hits);
                        BattleMenus.GetCurrentBattleMenu().Renzokeken.Show();
                        if (willfinish)
                        {
                            List<Kernel_bin.Renzokeken_Finisher> flags = Enum.GetValues(typeof(Kernel_bin.Renzokeken_Finisher))
                                .Cast<Kernel_bin.Renzokeken_Finisher>()
                                .Where(f => (f & r) != 0)
                                .ToList();
                            Kernel_bin.Renzokeken_Finisher finisher = choosefinish >= flags.Count ? flags.Last() : flags[choosefinish];
                            Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} hits {hits} times with {Command.Name}({Command.ID}) then uses {Kernel_bin.RenzokukenFinishersData[finisher].Name}.");
                        }
                        else
                            Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} hits {hits} times with {Command.Name}({Command.ID}) then fails to use a finisher.");
                    }
                    return true;
                }

                bool Command06_DRAW()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    //draw
                    //spawn a 1 page 4 row pool of the magic/gfs that the selected enemy has.
                    if (d.First().GetType() == typeof(Enemy))
                    {
                        var e = (Enemy)d.First();
                        DrawMagic(e.DrawList);
                        Draw_Pool.Refresh(e.DrawList);
                        Draw_Pool.Show();
                    }
                    return true;
                }

                bool Command07_DEVOUR()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    //TODO add devour commands
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                //bool Command08_UNNAMED() => throw new NotImplementedException();

                //bool Command09_CAST() => throw new NotImplementedException();

                //bool Command10_STOCK() => throw new NotImplementedException();

                bool Command11_DUEL()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command12_MUG()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    if (d.First().GetType() == typeof(Enemy))
                    {
                        var e = (Enemy)d.First();
                        //unsure if party member being ejected or if they need to be in the party for rare item to work
                        Saves.Item i = e.Mug(Memory.State[fromvc].SPD, Memory.State.PartyHasAbility(Kernel_bin.Abilities.RareItem));
                        Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} stole {i.DATA?.Name}({i.ID}) x {i.QTY} from { DebugMessageSuffix(d) }");
                    }
                    Damageable.EndTurn();
                    return true;
                }

                //bool Command13_NOMSG() => throw new NotImplementedException();

                bool Command14_SHOT() 
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command15_BLUE_MAGIC()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} casts {BlueMagic.Name}({BlueMagic.ID}) spell on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();
                    return false;
                }

                //bool Command16_SLOT() => throw new NotImplementedException();

                bool Command17_FIRE_CROSS_NO_MERCY()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command18_SORCERY_ICE_STRIKE()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command19_COMBINE()
                {
                    //perform angelo attack unless angel wing is unlocked and chosen in menu.
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command20_DESPERADO()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command21_BLOOD_PAIN()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command22_MASSIVE_ANCHOR()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                //bool Command23_DEFEND() => throw new NotImplementedException();

                bool Command24_MADRUSH()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command25_TREATMENT()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command26_RECOVERY()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command27_REVIVE()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }
                bool Command28_DARKSIDE()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Damageable.EndTurn();
                    return true;
                }

                bool Command29_CARD()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    if (d.First().GetType() == typeof(Enemy))
                    {
                        var e = (Enemy)d.First();
                        Cards.ID c = e.Card();
                        if (c == Cards.ID.Fail)

                            Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} Failed to use {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                        else if (c == Cards.ID.Immune)
                            Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} Failed to use {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) } because they are immune!");
                        else
                            Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) } and got a {c} card");
                        Damageable.EndTurn();
                    }
                    return true;
                }

                bool Command30_DOOM()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);
                    Damageable.EndTurn();
                    return true;
                }

                bool Command31_KAMIKAZI()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command32_ABSORB()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command33_LVL_DOWN()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command34_LVL_UP()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }

                bool Command35_SINGLE() => Command02_MAGIC();

                bool Command36_DOUBLE() {
                    // CHOOSE 2X TARGETS
                    throw new NotImplementedException();
                }

                bool Command37_TRIPLE()
                {
                    // CHOOSE 3X TARGETS
                    throw new NotImplementedException();
                }
                bool Command38_MINIMOG()
                {
                    Neededvaribles(out Damageable[] d, out Characters fromvc);

                    Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} used {Command.Name}({Command.ID}) on { DebugMessageSuffix(d) }");
                    Damageable.EndTurn();

                    return true;
                }
            }

            #endregion Methods

            #region Constructors

            public IGMData_TargetGroup(Damageable damageable, bool makesubs = true)
            {
                
                const int X = 25;
                const int w = 380;
                const int w2 = 210;
                const int h = 140;
                const int Y1 = 630 - h;
                CONTAINER.Pos = new Rectangle(X, Y1, w + w2, h);
                Init(new IGMData[]{
                    new IGMData_TargetEnemies(new Rectangle(CONTAINER.Pos.X, CONTAINER.Pos.Y, w, h)),
                    new IGMData_TargetParty(new Rectangle(CONTAINER.Pos.X + w, CONTAINER.Pos.Y, w2, h)),
                    makesubs ? new IGMData_Draw_Pool(new Rectangle(X +50, Y - 50, 300, 192), Damageable, true): null,
                    //makesubs ? new IGMData_BlueMagic_Pool(new Rectangle(X +50, Y - 50, 300, 192), Character, VisibleCharacter, true): null
                }, true);
                after();
            }

            #endregion Constructors

            public Kernel_bin.Blue_magic_Quistis_limit_break BlueMagic { get; private set; }

            public Kernel_bin.Battle_Commands Command { get; private set; }

            public Item_In_Menu Item { get; private set; }

            public Kernel_bin.Magic_Data Magic { get; private set; }
            public Kernel_bin.Target Target { get; private set; }

            public override void Draw() => base.Draw();

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

            public override void Refresh(Damageable damageable)
            {
                base.Refresh(damageable);
                Draw_Pool?.Refresh(damageable);
            }

            public override void Reset()
            {
                HideChildren();
                Hide();
                base.Reset();
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
        }

        #endregion Classes
    }
}