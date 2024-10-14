using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenVIII.AV;
using OpenVIII.Encoding.Tags;
using OpenVIII.Kernel;

namespace OpenVIII
{
    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public class BattleMenus : Menu
    {
        #region Fields

        private IReadOnlyDictionary<Mode, Action> _drawActions;
        private IReadOnlyDictionary<Mode, Func<bool>> _inputFunctions;
        private Module _lastGameState;
        private MenuModule.Mode _lastMenu;
        private ushort _lastMusic;
        private bool _lastMusicPlaying;
        private IReadOnlyDictionary<Mode, Action> _returnAction;
        private IReadOnlyDictionary<Mode, Func<bool>> _updateFunctions;

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
            GameOver
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
            Victory
        }

        #endregion Enums

        #region Properties

        protected byte Player { get; set; }

        /// <summary>
        /// Get Damageable from active battle menu;
        /// </summary>
        /// <returns></returns>
        public Damageable GetDamageable() => GetCurrentBattleMenu()?.Damageable;

        public new sbyte? PartyPos
        {
            get
            {
                var bm = GetCurrentBattleMenu();
                if (bm?.Damageable?.GetBattleMode().Equals(Damageable.BattleMode.YourTurn) ?? false)
                {
                    return bm.PartyPos;
                }
                return null;
            }
        }

        public VictoryMenu VictoryMenu { get => (VictoryMenu)(Data[SectionName.Victory]); protected set => Data[SectionName.Victory] = value; }

        #endregion Properties

        #region Methods

        public static BattleMenus Create() => Create<BattleMenus>();

        /// <summary>
        /// Save pre battle state.
        /// </summary>
        public void CameFrom()
        {
            _lastMenu = Module.State;
            _lastGameState = Memory.Module;
            _lastMusic = Memory.MusicIndex;
            _lastMusicPlaying = Music.Playing;
        }

        public override void Draw()
        {

            if (GetMode() == null || _drawActions == null) return;
            if (_drawActions.ContainsKey((Mode)GetMode()))
                _drawActions[(Mode)GetMode()]();
        }

        public IEnumerable<BattleMenu> GetBattleMenus() => Data?.Where(m => m.Value is BattleMenu && m.Value?.Damageable != null).Select(x => x.Value as BattleMenu);

        public BattleMenu GetCurrentBattleMenu() => (BattleMenu)Data?[PossibleValidPlayer()];

        public override bool Inputs()
        {
            var ret = false;
            InputMouse.Mode = MouseLockMode.Screen;
            Memory.IsMouseVisible = true;
            if (_inputFunctions?.ContainsKey((Mode)GetMode()) ?? false)
                ret = _inputFunctions[(Mode)GetMode()]();

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

        private void InitDictionaries()
        {
            if (_updateFunctions == null)
                _updateFunctions = new Dictionary<Mode, Func<bool>>
                {
                    {Mode.Starting, UpdateStartingFunction},
                    {Mode.Battle, UpdateBattleFunction},
                    {Mode.Victory, UpdateVictoryFunction},
                    {Mode.GameOver, UpdateGameOverFunction}
                };
            if (_drawActions == null)
                _drawActions = new Dictionary<Mode, Action>
                {
                    {Mode.Starting, DrawStartingAction},
                    {Mode.Battle, DrawBattleAction},
                    {Mode.Victory, DrawVictoryAction},
                    {Mode.GameOver, DrawGameOverAction}
                };
            if (_inputFunctions == null)
                _inputFunctions = new Dictionary<Mode, Func<bool>>
                {
                    //{Mode.Starting, InputStartingFunction},
                    {Mode.Battle, InputBattleFunction},
                    {Mode.Victory, InputVictoryFunction}
                    //{Mode.GameOver, InputGameOverFunction},
                };
            if (_returnAction == null)
                _returnAction = new Dictionary<Mode, Action>
                {
                    {Mode.Starting, ReturnStartingFunction},
                    {Mode.Battle, ReturnBattleFunction},
                    {Mode.Victory, ReturnVictoryFunction},
                    {Mode.GameOver, ReturnGameOverFunction}
                };
        }

        private void SetEnemyParty()
        {
            if (Enemy.Party == null || Enemy.Party.Count <= 0) return;
            byte i = 0;
            foreach (var e in Enemy.Party)
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

        private void SetParty()
        {
            if (Memory.State == null || !Memory.State.Characters || Memory.State.CharactersCount <= 0 ||
                Memory.State.Party == null) return;
            var i = 0;
            var party = Memory.State.Party.Select((element, index) => new {element, index})
                .ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList()
                .AsReadOnly();
            for (; i < party.Count; i++)
            {
                Data[SectionName.Party1 + i].SetDamageable(Memory.State[party[i].Value]);
                Data[SectionName.Party1 + i].Show();
            }

            for (; i <= (int)SectionName.Party3; i++)
            {
                Data[SectionName.Party1 + i].SetDamageable(null, forcenull: true);
                Data[SectionName.Party1 + i].Hide();
            }
        }

        /// <summary>
        /// Go back to pre battle state.
        /// </summary>
        public void ReturnTo()
        {
            Module.State = _lastMenu;
            Memory.Module = _lastGameState;
            IGM.Refresh(); // else the menu stats won't update.
            ModuleBattleDebug.ResetState();
            if (_lastMusicPlaying)
                Music.Play(_lastMusic);
            else
                Music.Stop();
        }

        public override bool SetMode(Enum mode)
        {
            if (!(base.GetMode()?.Equals(mode) ?? false))
                return base.SetMode(mode);
            return false;
        }

        public override bool Update()
        {
            var ret = false;
            if (GetMode() == null) return false;
            if (_updateFunctions != null && _updateFunctions.TryGetValue((Mode)GetMode(), out var u))
            {
                ret = u();
            }
            SkipFocus = true;
            skipdata = true;
            ret = base.Update() || ret;
            skipdata = false;
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
            InitDictionaries();
        }

        private bool BoolBattleMenu() => Data?.Any(m => m.Value.GetType() == typeof(BattleMenu) && m.Value.Enabled) ?? false;

        private bool BoolRenzokuken() => GetBattleMenus()?.Any(m => m.Enabled && (m.Renzokuken?.Enabled ?? false)) ?? false;

        private bool BoolShot() => GetBattleMenus()?.Any(m => m.Enabled && (m.Shot?.Enabled ?? false)) ?? false;

        private void DrawBattleAction()
        {
            StartDraw();
            //Had to split up the HP and Commands drawing. So that Commands would draw over HP.
            if (BoolRenzokuken())
                GetOneRenzokuken().DrawData(BattleMenu.SectionName.Renzokuken);

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

        private static void DrawGameOverAction()
        {
        }

        private static void DrawStartingAction()
        {
        }

        private void DrawVictoryAction() => VictoryMenu.Draw();

        private BattleMenu GetOneRenzokuken() => GetBattleMenus()?.First(m => m.Enabled && m.Renzokuken.Enabled);
        private BattleMenu GetOneShot() => GetBattleMenus()?.First(m => m.Enabled && m.Shot.Enabled);

        private bool InputBattleFunction()
        {
            if (BoolRenzokuken())
            {
                return GetOneRenzokuken().Inputs();
            }
            if (GetBattleMenus().Where(m => m.Damageable.GetBattleMode().Equals(Damageable.BattleMode.YourTurn)).Select(m => m.Inputs()).Any(ret => ret))
                return true;

            if (!Input2.DelayedButton(FF8TextTagKey.Cancel)) return false;
            if (!(GetCurrentBattleMenu().Damageable?.Switch() ?? true)) return false;
            var cnt = 0;
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

            return false;
        }

        private bool InputVictoryFunction() => VictoryMenu?.Inputs() ?? false;

        private void NewTurnSND()
        {
            Sound.Play(((BattleMenu) Data[PossibleValidPlayer()]).CrisisLevel > -1 ? 94 : 14);
        }

        private SectionName PossibleValidPlayer() => SectionName.Party1 + MathHelper.Clamp(Player, 0, (int)SectionName.Enemy8);

        private static void ReturnBattleFunction()
        {
        }

        private static void ReturnGameOverFunction()
        {
        }

        private static void ReturnStartingFunction()
        {
        }

        private static void ReturnVictoryFunction()
        {
        }

        private void TriggerVictory(ConcurrentDictionary<Characters, int> extraExp = null)
        {
            //if (extraExp == null) throw new ArgumentNullException(nameof(extraExp));
            var exp = 0;
            uint ap = 0;
            var items = new ConcurrentDictionary<byte, byte>();
            var cards = new ConcurrentDictionary<Cards.ID, byte>();
            foreach (var e in Enemy.Party)
            {
                exp += e.EXP;
                ap += e.AP;
                var drop = e.Drop(Memory.State.PartyHasAbility(Abilities.RareItem));
                var cardDrops = e.CardDrop();
                if (drop.QTY > 0 && drop.ID > 0)
                    if (!items.TryAdd(drop.ID, drop.QTY))
                        items[drop.ID] += drop.QTY;
                if (cardDrops == Cards.ID.Fail || cardDrops == Cards.ID.Immune) continue;
                if (!cards.TryAdd(cardDrops, 1))
                    cards[cardDrops]++;
            }
            SetMode(Mode.Victory);
            VictoryMenu?.Refresh(exp, ap, extraExp, items, cards);
            VictoryMenu?.Show();
        }

        private bool UpdateBattleFunction()
        {
            var ret = false;
            var bml = GetBattleMenus().ToList();
            if (bml.Count == 0) return false;// issue here.
            foreach (var m in bml)
            {
                ret = m.Update() || ret;
            }
            if (!(GetCurrentBattleMenu()?.Damageable?.GetBattleMode().Equals(Damageable.BattleMode.YourTurn) ?? false))
            {
                var cnt = bml.Count;
                if (Player + 1 == cnt)
                    Player = 0;
                for (var i = Player; cnt > 0; cnt--)
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

        private static bool UpdateGameOverFunction()
        {
            Memory.Module = OpenVIII.Module.FieldDebug;
            Memory.FieldHolder.FieldID = 75; //game over
            Music.Play(0);
            Module.State = MenuModule.Mode.MainLobby;
            return true;
        }

        private static bool UpdateStartingFunction() => throw new NotImplementedException();

        private bool UpdateVictoryFunction()
        {
            Music.Play(1);
            return VictoryMenu.Update();
        }

        #endregion Methods
    }
}
