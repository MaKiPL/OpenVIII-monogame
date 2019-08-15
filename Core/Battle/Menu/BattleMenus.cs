using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public partial class BattleMenus : Menus
    {
        #region Fields

        private int _player = 0;
        private Dictionary<Mode, Action> DrawActions;
        private Dictionary<Mode, Func<bool>> InputFunctions;
        private int lastgamestate;
        private Module_main_menu_debug.MainMenuStates lastmenu;
        private ushort lastmusic;
        private bool lastmusicplaying;
        private Dictionary<Mode, Action> ReturnAction;
        private Dictionary<Mode, Func<bool>> UpdateFunctions;

        #endregion Fields

        #region Enums

        public enum Mode : byte
        {
            Starting,
            Battle,
            Victory,
            GameOver,
        }

        public enum SectionName
        {
            Targets
        }

        #endregion Enums

        #region Properties

        public VictoryMenu Victory_Menu => (VictoryMenu)(menus?.Where(m => m.GetType().Equals(typeof(VictoryMenu))).First());

        public IGMData_TargetEnemies Target_Enemies => ((IGMData_TargetEnemies)((IGMDataItem_IGMData)Data[SectionName.Targets].ITEM[0, 0]).Data);
        public IGMData_TargetParty Target_Party => ((IGMData_TargetParty)((IGMDataItem_IGMData)Data[SectionName.Targets].ITEM[1, 0]).Data);

        #endregion Properties

        #region Methods

        public void CameFrom()
        {
            lastmenu = Module_main_menu_debug.State;
            lastgamestate = Memory.module;
            lastmusic = Memory.MusicIndex;
            lastmusicplaying = init_debugger_Audio.MusicPlaying;
        }

        public override void Draw()
        {
            if (DrawActions.ContainsKey((Mode)GetMode()))
                DrawActions[(Mode)GetMode()]();
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputFunctions.ContainsKey((Mode)GetMode()))
                ret = InputFunctions[(Mode)GetMode()]() && ret;
            if (Input2.DelayedButton(Keys.D1))
            {
                SetMode(Mode.Victory);
            }
            else if (Input2.DelayedButton(Keys.D2))
            {
                SetMode(Mode.GameOver);
            }
            return ret;
        }

        public override void Refresh()
        {
            if (Memory.State?.Characters != null)
            {
                IEnumerable<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank));
                int count = party.Count();
                menus = new List<Menu>(count);
                foreach (KeyValuePair<int, Characters> m in party)
                {
                    BattleMenu tmp = new BattleMenu(Memory.State.PartyData[m.Key], m.Value);
                    tmp.Hide();
                    menus.Add(tmp);
                }
                menus.Add(new VictoryMenu());
                SetMode(Mode.Battle);
                UpdateFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    {Mode.Starting, UpdateStartingFunction},
                    {Mode.Battle, UpdateBattleFunction},
                    {Mode.Victory, UpdateVictoryFunction},
                    {Mode.GameOver, UpdateGameOverFunction},
                };
                DrawActions = new Dictionary<Mode, Action>()
                {
                    {Mode.Starting, DrawStartingAction},
                    {Mode.Battle, DrawBattleAction},
                    {Mode.Victory, DrawVictoryAction},
                    {Mode.GameOver, DrawGameOverAction},
                };
                InputFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    //{Mode.Starting, InputStartingFunction},
                    {Mode.Battle, InputBattleFunction},
                    //{Mode.Victory, InputVictoryFunction},
                    //{Mode.GameOver, InputGameOverFunction},
                };
                ReturnAction = new Dictionary<Mode, Action>()
                {
                    {Mode.Starting, ReturnStartingFunction},
                    {Mode.Battle, ReturnBattleFunction},
                    {Mode.Victory, ReturnVictoryFunction},
                    {Mode.GameOver, ReturnGameOverFunction},
                };
                menus?.ForEach(m => m.Show());
            }
            Victory_Menu?.Refresh(10000, 1000, new Saves.Item(10, 100), new Saves.Item(20, 65), new Saves.Item(28, 54));
            base.Refresh();
        }

        public void ReturnTo()
        {
            Module_main_menu_debug.State = lastmenu;
            Memory.module = lastgamestate;
            if (lastmusicplaying)
                init_debugger_Audio.PlayMusic(lastmusic);
            else
                init_debugger_Audio.StopMusic();
        }

        public override bool Update()
        {
            bool ret = false;
            if (UpdateFunctions.TryGetValue((Mode)GetMode(), out Func<bool> u))
            {
                ret = u();
            }

            ret = base.Update() || ret;
            return ret;
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 881, Y = 636 };
            const int w = 380;
            const int h = 140;
            Data = new Dictionary<Enum, IGMData>()
            {
                {
                    SectionName.Targets, new IGMData_TargetGroup(
                    new IGMData_TargetEnemies(new Rectangle(25, (int)(Size.Y - h-6), w, h)) ,
                    new IGMData_TargetParty(new Rectangle(25+w, (int)(Size.Y -h- 6), 210, h)))
                }
            };
            Data.ForEach(x => x.Value.Show());
            base.Init();
        }

        private void DrawBattleAction()
        {
            StartDraw();
            //Had to split up the HP and Commands drawing. So that Commands would draw over HP.
            menus?.Where(m => m.GetType().Equals(typeof(BattleMenu))).ForEach(m => ((BattleMenu)m).DrawData(BattleMenu.SectionName.HP));
            menus?.Where(m => m.GetType().Equals(typeof(BattleMenu))).ForEach(m => ((BattleMenu)m).DrawData(BattleMenu.SectionName.Commands));
            DrawData();
            EndDraw();
        }

        private void DrawGameOverAction()
        {
        }

        private void DrawStartingAction()
        {
        }

        private void DrawVictoryAction() => Victory_Menu.Draw();

        private bool InputBattleFunction()
        {
            bool ret = false;
            if (Data[SectionName.Targets].Enabled)
            {
                return Data[SectionName.Targets].Inputs();
            }
            foreach (Menu m in menus.Where(m => m.GetType().Equals(typeof(BattleMenu)) && (BattleMenu.Mode)m.GetMode() == BattleMenu.Mode.YourTurn))
            {
                ret = m.Inputs() || ret;
                if (ret) return ret;
            }
            if (Input2.DelayedButton(FF8TextTagKey.Cancel))
            {
                switch ((BattleMenu.Mode)menus[_player].GetMode())
                {
                    case BattleMenu.Mode.YourTurn:
                        menus[_player].SetMode(BattleMenu.Mode.ATB_Charged);
                        break;
                }
                if (++_player > 2) _player = 0;
                menus[_player].SetMode(BattleMenu.Mode.YourTurn);
                if (((BattleMenu)menus[_player]).CrisisLevel)
                    init_debugger_Audio.PlaySound(94);
                else
                    init_debugger_Audio.PlaySound(14);
                switch ((BattleMenu.Mode)menus[_player].GetMode())
                {
                    case BattleMenu.Mode.ATB_Charged:
                        menus[_player].SetMode(BattleMenu.Mode.YourTurn);
                        break;
                }
            }
            return ret;
        }

        private bool InputGameOverFunction() => false;

        private bool InputStartingFunction() => false;

        private bool InputVictoryFunction() => throw new NotImplementedException();

        private void ReturnBattleFunction()
        {
        }

        private void ReturnGameOverFunction()
        {
        }

        private void ReturnStartingFunction()
        {
        }

        private void ReturnVictoryFunction()
        {
        }

        private bool UpdateBattleFunction()
        {
            menus?[_player].SetMode(BattleMenu.Mode.YourTurn);
            bool ret = false;
            foreach (Menu m in menus?.Where(m => m.GetType().Equals(typeof(BattleMenu))))
            {
                ret = m.Update() || ret;
            }
            return ret;
        }

        private bool UpdateGameOverFunction()
        {
            Memory.module = Memory.MODULE_FIELD_DEBUG;
            Memory.FieldHolder.FieldID = 75; //gover
            init_debugger_Audio.PlayMusic(0);
            Module_main_menu_debug.State = Module_main_menu_debug.MainMenuStates.MainLobby;
            return true;
        }

        private bool UpdateStartingFunction() => throw new NotImplementedException();

        private bool UpdateVictoryFunction()
        {
            init_debugger_Audio.PlayMusic(1);
            return Victory_Menu.Update();
        }

        #endregion Methods

        #region Classes

        public class IGMData_TargetGroup : IGMData_Group
        {
            public IGMData_TargetGroup(params IGMData[] d) : base(d)
            {
            }

            public override bool Inputs()
            {
                IGMData_TargetEnemies i = (IGMData_TargetEnemies)(((IGMDataItem_IGMData)ITEM[0, 0]).Data);
                IGMData_TargetParty ii = (IGMData_TargetParty)(((IGMDataItem_IGMData)ITEM[1, 0]).Data);
                bool ret = false;
                if (i.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !ii.Enabled))
                    i.Cursor_Status |= Cursor_Status.Enabled;
                else if (ii.Enabled && (((i.Cursor_Status | ii.Cursor_Status) & Cursor_Status.Enabled) == 0 || !i.Enabled))
                    ii.Cursor_Status |= Cursor_Status.Enabled;
                if (i.Enabled && (i.Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    ret = i.Inputs();
                }
                else if (ii.Enabled && (ii.Cursor_Status & Cursor_Status.Enabled) != 0)
                {
                    ret = ii.Inputs() || ret;
                }
                if(!ret)
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
                return base.Inputs_CANCEL();
            }
        }

        public class IGMData_TargetEnemies : IGMData
        {
            #region Constructors

            public IGMData_TargetEnemies(Rectangle pos) : base(6, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.TARGET), 2, 3)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (Memory.State?.Characters != null)
                {
                    int pos = 0;
                    foreach (Enemy e in Enemy.EnemyParty)
                    {
                        ITEM[pos, 0] = new IGMDataItem_String(e.Name, SIZE[pos], Font.ColorID.White);

                        pos++;
                    }
                }
            }

            public override void Inputs_Left()
            {
                if (CURSOR_SELECT < 3)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.CURSOR_SELECT = CURSOR_SELECT % 3;
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT - 3);
                }
                base.Inputs_Left();
            }
            public override void Inputs_Right()
            {
                if (CURSOR_SELECT >= 3 || (ITEM[3, 0] == null || !ITEM[3, 0].Enabled))
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.Cursor_Status |= Cursor_Status.Enabled;
                    Menu.BattleMenus.Target_Party.CURSOR_SELECT = CURSOR_SELECT % 3;
                }
                else
                {
                    SetCursor_select(CURSOR_SELECT + 3);
                }
                base.Inputs_Right();
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 7 * row + 2;
                //SIZE[i].Inflate(-22, -8);
                //SIZE[i].Offset(0, 12 + (-8 * row));
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            #endregion Methods
        }

        public class IGMData_TargetParty : IGMData
        {
            #region Constructors

            public IGMData_TargetParty(Rectangle pos) : base(3, 1, new IGMDataItem_Box(pos: pos, title: Icons.ID.NAME), 1, 3)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (Memory.State?.Characters != null)
                {
                    List<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
                    byte pos = 0;
                    foreach (KeyValuePair<int, Characters> pm in party)
                    {
                        Saves.CharacterData data = Memory.State[Memory.State.PartyData[pm.Key]];
                        bool ded = data.IsDead;
                        ITEM[pos, 0] = new IGMDataItem_String(Memory.State[pm.Value].Name, SIZE[pos], ded ? Font.ColorID.Dark_Gray : Font.ColorID.White);

                        pos++;
                    }
                }
            }

            protected override void InitShift(int i, int col, int row)
            {
                base.InitShift(i, col, row);
                SIZE[i].Inflate(-18, -20);
                SIZE[i].Y -= 7 * row + 2;
                //SIZE[i].Inflate(-22, -8);
                //SIZE[i].Offset(0, 12 + (-8 * row));
                SIZE[i].Height = (int)(12 * TextScale.Y);
            }

            public override void Inputs_Left()
            {
                Cursor_Status &= ~Cursor_Status.Enabled;
                Menu.BattleMenus.Target_Enemies.Cursor_Status |= Cursor_Status.Enabled;
                Menu.BattleMenus.Target_Enemies.CURSOR_SELECT = (CURSOR_SELECT +3) % 6;
                base.Inputs_Left();
            }

            public override void Inputs_Right()
            {
                Cursor_Status &= ~Cursor_Status.Enabled;
                Menu.BattleMenus.Target_Enemies.Cursor_Status |= Cursor_Status.Enabled;
                Menu.BattleMenus.Target_Enemies.CURSOR_SELECT = CURSOR_SELECT % 3;
                base.Inputs_Right();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}