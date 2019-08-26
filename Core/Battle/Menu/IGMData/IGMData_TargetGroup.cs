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
            public Kernel_bin.Battle_Commands Command { get; private set; }
            public Kernel_bin.Blue_magic_Quistis_limit_break BlueMagic { get; private set; }
            public Kernel_bin.Magic_Data Magic { get; private set; }

            #region Constructors

            public IGMData_TargetGroup(params IGMData[] d) : base(d)
            {
                after();
            }

            private void after()
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                i.Target_Party = ii;
                ii.Target_Enemies = i;
            }


            public IGMData_TargetGroup():base()
            {
                const int X = 25;
                const int w = 380;
                const int w2 = 210;
                const int h = 140;
                const int Y1 = 630 - h;
                CONTAINER.Pos = new Rectangle(X, Y1, w + w2, h);
                Init(new IGMData[]{ new IGMData_TargetEnemies(new Rectangle(CONTAINER.Pos.X, CONTAINER.Pos.Y, w, h)),
                        new IGMData_TargetParty(new Rectangle(CONTAINER.Pos.X + w, CONTAINER.Pos.Y, w2, h)) },true);
                after();
            }
            #endregion Constructors

            #region Methods

            public override bool Inputs()
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                bool ret = false;
                if (i.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !ii.Enabled))
                    i.Cursor_Status |= Cursor_Status.Enabled;
                else if (ii.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !i.Enabled))
                    ii.Cursor_Status |= Cursor_Status.Enabled;

                if (i.Enabled && ((i.Cursor_Status & Cursor_Status.Enabled) != 0 || i.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    i.Cursor_Status |= Cursor_Status.Enabled;
                    ii.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = i.Inputs();
                }
                if (!ret && ii.Enabled && ((ii.Cursor_Status & Cursor_Status.Enabled) != 0 || ii.CONTAINER.Pos.Contains(MouseLocation)))
                {
                    ii.Cursor_Status |= Cursor_Status.Enabled;
                    i.Cursor_Status &= ~Cursor_Status.Enabled;
                    ret = ii.Inputs();
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

            public void ShowTargetWindows()
            {
                skipdata = true;
                Show();
                skipdata = false;
                Refresh();
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

            private void SelectTargetWindows(Kernel_bin.Target t)
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                if ((t & Kernel_bin.Target.Ally) != 0)
                    ii.Show();
                else
                    ii.Hide();
                if ((t & Kernel_bin.Target.Enemy) != 0)
                    i.Show();
                else
                    i.Hide();
            }

            public override bool Inputs_CANCEL()
            {
                Hide();
                return true;
            }

            private readonly int[] Renzokuken_hits = { 4, 5, 6, 7 };

            protected override void Init()
            {
                base.Init();
                Hide();
            }
            public override bool Inputs_OKAY()
            {
                //TODO improve this.
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                Enemy e = Enemy.Party[ii.CURSOR_SELECT < Enemy.Party.Count ? ii.CURSOR_SELECT : Enemy.Party.Count-1 ];
                Characters c = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[ii.CURSOR_SELECT];
                Characters vc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[ii.CURSOR_SELECT];
                int p = BattleMenus.Player;
                Characters fromc = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[p];
                Characters fromvc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[p];
                bool enemytarget = false;
                if ((i.Cursor_Status & Cursor_Status.Enabled) != 0 && i.Enabled)
                    enemytarget = true;

                Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Command.Name}({Command.ID}) command on { (enemytarget ? $"{e.Name.Value_str}({i.CURSOR_SELECT})" : Memory.Strings.GetName(vc).Value_str) }");
                base.Inputs_OKAY();
                //spawn sub menu
                switch (Command.ID)
                {
                    case 1: //Attack
                        break;
                    case 5: //Renzokuken
                        byte w = Memory.State[fromvc].WeaponID;
                        int hits = Memory.State[fromvc].CurrentCrisisLevel < Renzokuken_hits.Length ? Renzokuken_hits[Memory.State[fromvc].CurrentCrisisLevel] : Renzokuken_hits.Last();
                        int finisherchance = (Memory.State[fromvc].CurrentCrisisLevel + 1) * 60;
                        bool willfinish = Memory.Random.Next(0, byte.MaxValue) <= finisherchance;
                        int choosefinish = Memory.Random.Next(0, 3);
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

                        break;

                    case 6: //draw
                        //spawn a 1 page 4 row pool of the magic/gfs that the selected enemy has.
                        BattleMenus.DrawMagic(e.DrawList);
                        break;

                    default:
                        break;
                }
                return true;
            }

            #endregion Methods
        }

        private void DrawMagic(Debug_battleDat.Magic[] drawList) =>
            // display pool with list.
            Debug.WriteLine($"Display draw pool: {string.Join(", ", drawList)}");

        #endregion Classes
    }
}