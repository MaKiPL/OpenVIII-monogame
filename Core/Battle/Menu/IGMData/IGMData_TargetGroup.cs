using System;
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

            private static void SelectTargetWindows(Kernel_bin.Target t)
            {
                if ((t & Kernel_bin.Target.Ally) != 0)
                    BattleMenus.Target_Party.Show();
                else
                    BattleMenus.Target_Party.Hide();
                if ((t & Kernel_bin.Target.Enemy) != 0)
                    BattleMenus.Target_Enemies.Show();
                else
                    BattleMenus.Target_Enemies.Hide();
            }

            public override bool Inputs_CANCEL()
            {
                Hide();
                return base.Inputs_CANCEL();
            }

            public override bool Inputs_OKAY()
            {
                //TODO improve this.
                Enemy e = Enemy.Party[BattleMenus.Target_Enemies.CURSOR_SELECT];
                Characters c = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[BattleMenus.Target_Enemies.CURSOR_SELECT];
                Characters vc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[BattleMenus.Target_Enemies.CURSOR_SELECT];
                int p = BattleMenus.Player;
                Characters fromc = Memory.State.PartyData.Where(x => x != Characters.Blank).ToList()[p];
                Characters fromvc = Memory.State.Party.Where(x => x != Characters.Blank).ToList()[p];
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                bool enemytarget = false;
                if ((i.Cursor_Status & Cursor_Status.Enabled) != 0 && i.Enabled)
                    enemytarget = true;

                Debug.WriteLine($"{Memory.Strings.GetName(fromvc)} uses {Command.Name}({Command.ID}) command on { (enemytarget ? e.Name : Memory.Strings.GetName(vc)) }");
                base.Inputs_OKAY();
                //spawn sub menu
                switch (Command.ID)
                {
                    case 1: //attack
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

        private void DrawMagic(Debug_battleDat.Magic[] drawList) => throw new NotImplementedException();

        #endregion Classes
    }
}