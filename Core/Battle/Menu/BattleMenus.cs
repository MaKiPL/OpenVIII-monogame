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
        private MODULE lastgamestate;
        private Module_main_menu_debug.MainMenuStates lastmenu;
        private ushort lastmusic;
        private bool lastmusicplaying;
        private Dictionary<Mode, Action> ReturnAction;
        private Dictionary<Mode, Func<bool>> UpdateFunctions;

        #endregion Fields

        #region Enums

        public enum Mode : byte
        {
            /// <summary>
            /// Spawning character's and enemies and flying the camera around
            /// </summary>
            Starting,
            /// <summary>
            /// running atb and using battle menus.
            /// </summary>
            Battle,
            /// <summary>
            /// Fade out and goto victory menu.
            /// </summary>
            Victory,
            /// <summary>
            /// Fade out and goto game over screen.
            /// </summary>
            GameOver,
        }

        public enum SectionName
        {
            /// <summary>
            /// Target window
            /// </summary>
            Targets
        }

        #endregion Enums

        #region Properties

        public VictoryMenu Victory_Menu => (VictoryMenu)(menus?.Where(m => m.GetType().Equals(typeof(VictoryMenu))).First());
        public IGMData_TargetGroup Target_Group => (IGMData_TargetGroup)Data[SectionName.Targets];
        public IGMData_TargetEnemies Target_Enemies => ((IGMData_TargetEnemies)((IGMDataItem_IGMData)Data[SectionName.Targets].ITEM[0, 0]).Data);
        public IGMData_TargetParty Target_Party => ((IGMData_TargetParty)((IGMDataItem_IGMData)Data[SectionName.Targets].ITEM[1, 0]).Data);

        #endregion Properties

        #region Methods

        /// <summary>
        /// Save pre battle state.
        /// </summary>
        public void CameFrom()
        {
            lastmenu = Module_main_menu_debug.State;
            lastgamestate = Memory.module;
            lastmusic = Memory.MusicIndex;
            lastmusicplaying = init_debugger_Audio.MusicPlaying;
        }

        public override void Draw()
        {
            if (GetMode() != null)
            {
                if (DrawActions.ContainsKey((Mode)GetMode()))
                    DrawActions[(Mode)GetMode()]();
            }
        }

        public override bool Inputs()
        {
            bool ret = false;
            InputMouse.Mode = MouseLockMode.Screen;
            Memory.IsMouseVisible = true;
            if (InputFunctions.ContainsKey((Mode)GetMode()))
                ret = InputFunctions[(Mode)GetMode()]() && ret;
            // press 1 to force victory
            if (Input2.DelayedButton(Keys.D6))
            {
                SetMode(Mode.Victory);
            }
            // press 2 to force game over
            else if (Input2.DelayedButton(Keys.D7))
            {
                SetMode(Mode.GameOver);
            }
            else if (Input2.DelayedButton(Keys.D9))
                ReturnTo();
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
            // exp, items and ap you are going to get after the battle is over.
            Victory_Menu?.Refresh(10000, 1000, new Saves.Item(10, 100), new Saves.Item(20, 65), new Saves.Item(28, 54));
            base.Refresh();
        }


        /// <summary>
        /// Go back to pre battle state.
        /// </summary>
        public void ReturnTo()
        {
            Module_main_menu_debug.State = lastmenu;
            Memory.module = lastgamestate;
            Module_battle_debug.ResetState();
            if (lastmusicplaying)
                init_debugger_Audio.PlayMusic(lastmusic);
            else
                init_debugger_Audio.StopMusic();
        }

        public override bool Update()
        {
            
            bool ret = false;
            if (GetMode() != null)
            {
                if (UpdateFunctions.TryGetValue((Mode)GetMode(), out Func<bool> u))
                {
                    ret = u();
                }

                ret = base.Update() || ret;
            }
            return ret;
        }

        protected override void Init()
        {
            NoInputOnUpdate = true;
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
            Data.ForEach(x => x.Value.Hide());
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
                do
                {
                    if (++_player > 2) _player = 0;
                }
                while (menus.Count <= _player || menus[_player] == null|| menus[_player].GetType() != typeof(BattleMenu));
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
            Memory.module = MODULE.FIELD_DEBUG;
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
    }
}