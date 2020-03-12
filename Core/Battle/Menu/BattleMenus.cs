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
    public partial class BattleMenus : Menu
    {
        #region Fields

        private Dictionary<Mode, Action> DrawActions;
        private Dictionary<Mode, Func<bool>> InputFunctions;
        private MODULE lastgamestate;
        private MenuModule.Mode lastmenu;
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
            /// Party Member Menu
            /// </summary>
            Party1,

            /// <summary>
            /// Party Member Menu
            /// </summary>
            Party2,

            /// <summary>
            /// Party Member Menu
            /// </summary>
            Party3,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy1,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy2,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy3,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy4,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy5,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy6,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy7,

            /// <summary>
            /// Debug Enemy Menu
            /// </summary>
            Enemy8,

            /// <summary>
            /// Victory Menu
            /// </summary>
            Victory,
        }

        #endregion Enums

        #region Properties

        protected byte Player { get; set; } = 0;

        /// <summary>
        /// Get Damageable from active battle menu;
        /// </summary>
        /// <returns></returns>
        public Damageable GetDamageable() => GetCurrentBattleMenu()?.Damageable;

        public new sbyte? PartyPos
        {
            get
            {
                BattleMenu bm = GetCurrentBattleMenu();
                if (bm?.Damageable?.GetBattleMode().Equals(Damageable.BattleMode.YourTurn) ?? false)
                {
                    return bm.PartyPos;
                }
                return null;
            }
        }

        public VictoryMenu Victory_Menu { get => (VictoryMenu)(Data[SectionName.Victory]); protected set => Data[SectionName.Victory] = value; }

        #endregion Properties

        #region Methods

        public static BattleMenus Create() => Create<BattleMenus>();

        /// <summary>
        /// Save pre battle state.
        /// </summary>
        public void CameFrom()
        {
            lastmenu = Menu.Module.State;
            lastgamestate = Memory.Module;
            lastmusic = Memory.MusicIndex;
            lastmusicplaying = AV.Music.Playing;
        }

        public override void Draw()
        {
            if (GetMode() != null && DrawActions != null)
            {
                if (DrawActions.ContainsKey((Mode)GetMode()))
                    DrawActions[(Mode)GetMode()]();
            }
        }

        public IEnumerable<BattleMenu> GetBattleMenus() => Data?.Where(m => m.Value.GetType().Equals(typeof(BattleMenu)) && m.Value.Damageable != null).Select(x => (BattleMenu)x.Value);

        public BattleMenu GetCurrentBattleMenu() => (BattleMenu)Data?[PossibleValidPlayer()];

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
            SetParty();
            SetEnemyParty();
            SetMode(Mode.Battle);

            // exp, items and ap you are going to get after the battle is over.
            base.Refresh();
        }

        private void InitDictionarys()
        {
            if (UpdateFunctions == null)
                UpdateFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    {Mode.Starting, UpdateStartingFunction},
                    {Mode.Battle, UpdateBattleFunction},
                    {Mode.Victory, UpdateVictoryFunction},
                    {Mode.GameOver, UpdateGameOverFunction},
                };
            if (DrawActions == null)
                DrawActions = new Dictionary<Mode, Action>()
                {
                    {Mode.Starting, DrawStartingAction},
                    {Mode.Battle, DrawBattleAction},
                    {Mode.Victory, DrawVictoryAction},
                    {Mode.GameOver, DrawGameOverAction},
                };
            if (InputFunctions == null)
                InputFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    //{Mode.Starting, InputStartingFunction},
                    {Mode.Battle, InputBattleFunction},
                    {Mode.Victory, InputVictoryFunction},
                    //{Mode.GameOver, InputGameOverFunction},
                };
            if (ReturnAction == null)
                ReturnAction = new Dictionary<Mode, Action>()
                {
                    {Mode.Starting, ReturnStartingFunction},
                    {Mode.Battle, ReturnBattleFunction},
                    {Mode.Victory, ReturnVictoryFunction},
                    {Mode.GameOver, ReturnGameOverFunction},
                };
        }

        private void SetEnemyParty()
        {
            if (Enemy.Party != null && Enemy.Party.Count > 0)
            {
                byte i = 0;
                foreach (Enemy e in Enemy.Party)
                {
                    Data[SectionName.Enemy1 + i].SetDamageable(e);
                    Data[SectionName.Enemy1 + i].Show();
                    i++;
                }

                for (; i < 8; i++)
                {
                    Data[SectionName.Enemy1 + i].SetDamageable(null, forcenull: true);
                    Data[SectionName.Enemy1 + i].Hide();
                }
            }
        }

        private void SetParty()
        {
            if (Memory.State?.Characters != null && Memory.State.Characters.Count > 0 && Memory.State.Party != null)
            {
                byte i = 0;
                IEnumerable<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank));
                int count = party.Count();

                foreach (KeyValuePair<int, Characters> m in party)
                {
                    Data[SectionName.Party1 + i].SetDamageable(Memory.State[Memory.State.PartyData[m.Key]]);
                    Data[SectionName.Party1 + i].Show();
                    i++;
                }
                for (; i <= (int)SectionName.Party3; i++)
                {
                    Data[SectionName.Party1 + i].SetDamageable(null, forcenull: true);
                    Data[SectionName.Party1 + i].Hide();
                }
            }
        }

        /// <summary>
        /// Go back to pre battle state.
        /// </summary>
        public void ReturnTo()
        {
            Menu.Module.State = lastmenu;
            Memory.Module = lastgamestate;
            Menu.IGM.Refresh(); // else the menu stats won't update.
            ModuleBattleDebug.ResetState();
            if (lastmusicplaying)
                AV.Music.Play(lastmusic);
            else
                AV.Music.Stop();
        }

        public override bool SetMode(Enum mode)
        {
            if (!(base.GetMode()?.Equals(mode) ?? false))
                return base.SetMode(mode);
            return false;
        }

        public override bool Update()
        {
            bool ret = false;
            if (GetMode() != null)
            {
                if (UpdateFunctions != null && UpdateFunctions.TryGetValue((Mode)GetMode(), out Func<bool> u))
                {
                    ret = u();
                }
                SkipFocus = true;
                skipdata = true;
                ret = base.Update() || ret;
                skipdata = false;
            }
            return ret;
        }

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 881, Y = 636 };
            base.Init();
            Data.TryAdd(SectionName.Party1, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Party2, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Party3, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy1, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy2, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy3, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy4, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy5, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy6, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy7, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Enemy8, BattleMenu.Create(null));
            Data.TryAdd(SectionName.Victory, VictoryMenu.Create());
            Data.ForEach(x => x.Value.Hide());
            InitDictionarys();
        }

        private bool BoolBattleMenu() => Data?.Any(m => m.Value.GetType().Equals(typeof(BattleMenu)) && m.Value.Enabled) ?? false;

        private bool BoolRenzokeken() => GetBattleMenus()?.Any(m => m.Enabled && (m.Renzokeken?.Enabled ?? false)) ?? false;

        private bool BoolShot() => GetBattleMenus()?.Any(m => m.Enabled && (m.Shot?.Enabled ?? false)) ?? false;
        public override void StartDraw()
        {
            //if (BoolShot())
            //    GetOneShot().Shot.DrawCrosshair();
            base.StartDraw();
        }

        private void DrawBattleAction()
        {
            StartDraw();
            //Had to split up the HP and Commands drawing. So that Commands would draw over HP.
            if (BoolRenzokeken())
                GetOneRenzokeken().DrawData(BattleMenu.SectionName.Renzokeken);

            else if (BoolShot())
                GetOneShot().DrawData(BattleMenu.SectionName.Shot);
            else if (BoolBattleMenu())
            {
                GetBattleMenus().ForEach(m => m.DrawData(BattleMenu.SectionName.HP));
                GetBattleMenus().ForEach(m => m.DrawData(BattleMenu.SectionName.Commands));
            }
            //DrawData();
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
        private BattleMenu GetOneShot() => GetBattleMenus()?.First(m => m.Enabled && m.Shot.Enabled);

        private bool InputBattleFunction()
        {
            bool ret = false;
            if (BoolRenzokeken())
            {
                return GetOneRenzokeken().Inputs();
            }
            foreach (BattleMenu m in GetBattleMenus().Where(m => m.Damageable.GetBattleMode().Equals(Damageable.BattleMode.YourTurn)))
            {
                ret = m.Inputs() || ret;
                if (ret) return ret;
            }
            if (Input2.DelayedButton(FF8TextTagKey.Cancel))
            {
                if (GetCurrentBattleMenu().Damageable?.Switch() ?? true)
                {
                    int cnt = 0;
                    do
                    {
                        if (++Player > (int)SectionName.Enemy8) Player = 0;
                        if (++cnt > (int)SectionName.Enemy8 * 2) return false;
                    }
                    while (Data.Count <= Player ||
                    Data[PossibleValidPlayer()]?.Damageable == null ||
                    Data[PossibleValidPlayer()].GetType() != typeof(BattleMenu) ||
                    !GetCurrentBattleMenu().Damageable.StartTurn());
                    NewTurnSND();
                }
            }

            return ret;
        }

        private bool InputGameOverFunction() => false;

        private bool InputStartingFunction() => false;

        private bool InputVictoryFunction() => Victory_Menu?.Inputs() ?? false;

        private void NewTurnSND()
        {
            if (((BattleMenu)Data[PossibleValidPlayer()]).CrisisLevel > -1)
                AV.Sound.Play(94);
            else
                AV.Sound.Play(14);
        }

        private SectionName PossibleValidPlayer() => SectionName.Party1 + MathHelper.Clamp(Player, 0, (int)SectionName.Enemy8);

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
                Saves.Item drop = e.Drop(Memory.State.PartyHasAbility(Kernel.Abilities.RareItem));
                Cards.ID carddrop = e.CardDrop();
                if (drop.QTY > 0 && drop.ID > 0)
                    if (!items.TryAdd(drop.ID, drop.QTY))
                        items[drop.ID] += drop.QTY;
                if (carddrop != Cards.ID.Fail && carddrop != Cards.ID.Immune)
                    if (!cards.TryAdd(carddrop, 1))
                        cards[carddrop]++;
            }
            SetMode(Mode.Victory);
            Victory_Menu?.Refresh(exp, ap, expextra, items, cards);
            Victory_Menu?.Show();
        }

        private bool UpdateBattleFunction()
        {
            bool ret = false;
            List<BattleMenu> bml = GetBattleMenus().ToList();
            if (bml.Count == 0) return false;// issue here.
            foreach (BattleMenu m in bml)
            {
                ret = m.Update() || ret;
            }
            if (!(GetCurrentBattleMenu()?.Damageable?.GetBattleMode().Equals(Damageable.BattleMode.YourTurn) ?? false))
            {
                int cnt = bml.Count;
                if (Player + 1 == cnt)
                    Player = 0;
                for (byte i = Player; cnt > 0; cnt--)
                {
                    if (i < bml.Count && bml[i].Damageable.StartTurn())
                    {
                        Player = i;
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
            Memory.Module = MODULE.FIELD_DEBUG;
            Memory.FieldHolder.FieldID = 75; //gover
            AV.Music.Play(0);
            Menu.Module.State = MenuModule.Mode.MainLobby;
            return true;
        }

        private bool UpdateStartingFunction() => throw new NotImplementedException();

        private bool UpdateVictoryFunction()
        {
            AV.Music.Play(1);
            return Victory_Menu.Update();
        }

        #endregion Methods
    }
}