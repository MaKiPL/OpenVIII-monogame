using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Concurrent;
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

        #region Methods

        private bool BoolBattleMenu() => menus?.Any(m => m.GetType().Equals(typeof(BattleMenu)) && m.Enabled) ?? false;

        private bool BoolRenzokeken() => GetBattleMenus()?.Any(m => m.Enabled && m.Renzokeken.Enabled) ?? false;

        private void DrawBattleAction()
        {
            StartDraw();
            //Had to split up the HP and Commands drawing. So that Commands would draw over HP.
            if (BoolRenzokeken())
                GetOneRenzokeken().DrawData(BattleMenu.SectionName.Renzokeken);
            else if (BoolBattleMenu())
            {
                GetBattleMenus().ForEach(m => m.DrawData(BattleMenu.SectionName.HP));
                GetBattleMenus().ForEach(m => m.DrawData(BattleMenu.SectionName.Commands));
            }
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

        private BattleMenu GetOneRenzokeken() => GetBattleMenus()?.First(m => m.Enabled && m.Renzokeken.Enabled);

        private bool InputBattleFunction()
        {
            bool ret = false;
            if (BoolRenzokeken())
            {
                return GetOneRenzokeken().Inputs();
            }
            foreach (Menu m in menus.Where(m => m.GetType().Equals(typeof(BattleMenu)) && (BattleMenu.Mode)m.GetMode() == BattleMenu.Mode.YourTurn))
            {
                ret = m.Inputs() || ret;
                if (ret) return ret;
            }
            if (Input2.DelayedButton(FF8TextTagKey.Cancel))
            {
                switch (GetCurrentBattleMenu().GetMode())
                {
                    case BattleMenu.Mode.YourTurn:
                        menus[_player].SetMode(BattleMenu.Mode.ATB_Charged);
                        break;
                }
                int cnt = 0;
                do
                {
                    if (++_player > 2) _player = 0;
                    if (++cnt > 6) return false;
                }
                while (menus.Count <= _player || menus[_player] == null || menus[_player].GetType() != typeof(BattleMenu) || !((BattleMenu)menus[_player]).GetMode().Equals(BattleMenu.Mode.ATB_Charged));
                //menus[_player].SetMode(BattleMenu.Mode.YourTurn);
                NewTurnSND();
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

        private void NewTurnSND()
        {
            if (((BattleMenu)menus[_player]).CrisisLevel)
                init_debugger_Audio.PlaySound(94);
            else
                init_debugger_Audio.PlaySound(14);
        }

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

        private void TriggerVictory(ConcurrentDictionary<Characters, int> expextra = null)
        {
            int exp = 0;
            uint ap = 0;
            ConcurrentDictionary<byte, byte> items = new ConcurrentDictionary<byte, byte>();
            ConcurrentDictionary<Cards.ID, byte> cards = new ConcurrentDictionary<Cards.ID, byte>();
            foreach (Enemy e in Enemy.Party)
            {
                exp += e.EXP;
                ap += e.AP;
                Saves.Item drop = e.Drop(Memory.State.PartyHasAbility(Kernel_bin.Abilities.RareItem));
                Cards.ID carddrop = e.CardDrop();
                if (drop.QTY > 0 && drop.ID > 0)
                    if (!items.TryAdd(drop.ID, drop.QTY))
                        items[drop.ID]+= drop.QTY;
                if (carddrop != Cards.ID.Fail && carddrop != Cards.ID.Immune)
                    if (!cards.TryAdd(carddrop, 1))
                        cards[carddrop]++;
            }
            SetMode(Mode.Victory);
            Victory_Menu?.Refresh(exp, ap, expextra, items,cards);
        }


        private bool UpdateBattleFunction()
        {
            bool ret = false;
            foreach (Menu m in menus?.Where(m => m.GetType().Equals(typeof(BattleMenu))))
            {
                ret = m.Update() || ret;
            }
            List<BattleMenu> bml = GetBattleMenus().ToList();
            if (!(menus?[_player].GetMode().Equals(BattleMenu.Mode.YourTurn) ?? false))
            {
                if (_player + 1 == bml.Count)
                    _player = 0;
                int cnt = 3;
                for (int i = _player; cnt >0; cnt--)
                {
                    if (bml[i].GetMode().Equals(BattleMenu.Mode.ATB_Charged))
                    {
                        bml[i].SetMode(BattleMenu.Mode.YourTurn);
                        _player = i;
                        NewTurnSND();
                        break;
                    }
                    i++;
                    if (i >= bml.Count)
                        i = 0;
                }
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

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 881, Y = 636 };
            Data.ForEach(x => x.Value.Hide());
            base.Init();
        }

        #endregion Methods

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

        public int Player { get => _player; protected set => _player = value; }

        public VictoryMenu Victory_Menu => (VictoryMenu)(menus?.Where(m => m.GetType().Equals(typeof(VictoryMenu))).First());

        #endregion Properties

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

        public void EndTurn()
        {
            GetCurrentBattleMenu().SetMode(BattleMenu.Mode.ATB_Charging);
            GetCurrentBattleMenu().Reset();
            GetCurrentBattleMenu().Refresh();
        }

        public IEnumerable<BattleMenu> GetBattleMenus() => menus?.Where(m => m.GetType().Equals(typeof(BattleMenu))).Select(x => (BattleMenu)x);

        public BattleMenu GetCurrentBattleMenu() => (BattleMenu)menus?[_player];

        public override bool Inputs()
        {
            bool ret = false;
            InputMouse.Mode = MouseLockMode.Screen;
            Memory.IsMouseVisible = true;
            if (InputFunctions?.ContainsKey((Mode)GetMode()) ?? false)
                ret = InputFunctions[(Mode)GetMode()]() && ret;
            // press 6 to force victory
            if (Input2.DelayedButton(Keys.D6))
            {
                TriggerVictory();
            }
            // press 7 to force game over
            else if (Input2.DelayedButton(Keys.D7))
            {
                SetMode(Mode.GameOver);
            }
            // press 9 to go back to last state.
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
                    BattleMenu tmp = new BattleMenu(Memory.State[Memory.State.PartyData[m.Key]]);
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
            base.Refresh();
        }

        /// <summary>
        /// Go back to pre battle state.
        /// </summary>
        public void ReturnTo()
        {
            Module_main_menu_debug.State = lastmenu;
            Memory.module = lastgamestate;
            Menu.IGM.Refresh(); // else the menu stats won't update.
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
    }
}