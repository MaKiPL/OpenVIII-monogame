using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenVIII.Encoding.Tags;

namespace OpenVIII
{
    /// <summary>
    /// Character BattleMenu
    /// </summary>
    public class BattleMenu : Menu
    {
        //private Mode _mode = Mode.Waiting;

        #region Constructors

        public BattleMenu(Characters character, Characters? visablecharacter = null) : base(character, visablecharacter)
        {
        }

        #endregion Constructors

        #region Enums

        public enum Mode : byte
        {
            /// <summary>
            /// ATB bar filling
            /// </summary>
            /// <remarks>Orange Bar precent filled. ATB/Full ATB</remarks>
            ATB_Charging,

            /// <summary>
            /// ATB bar charged, waiting for your turn
            /// </summary>
            /// <remarks>Yellow Bar</remarks>
            ATB_Charged,

            /// <summary>
            /// Your turn
            /// </summary>
            /// <remarks>Yellow Bar/Name/HP Blinking</remarks>
            YourTurn,

            /// <summary>
            /// GF Cast
            /// </summary>
            /// <remarks>Show GF name/hp and blueish bar.</remarks>
            GF_Charging,
        }

        public enum SectionName : byte
        {
            Commands,
            HP
        }

        #endregion Enums

        #region Methods

        /// <summary>
        /// <para>Draws the IGMData</para>
        /// <para>Skips Start and Stop because this class should be in another class</para>
        /// </summary>
        public override void Draw() => base.DrawData();

        public override bool Inputs() => Data[SectionName.Commands].Inputs();

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2 { X = 881, Y = 636 };
            Data.Add(SectionName.Commands, new IGMData_Commands(new Rectangle(50, (int)(Size.Y - 204), 210, 192), Character, VisableCharacter, true));
            Data.Add(SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X - 389), 507, 389, 126), Character, VisableCharacter));
            Data.ForEach(x=>x.Value.SetModeChangeEvent(ref ModeChangeHandler));
            SetMode(Mode.ATB_Charging);
            base.Init();
        }

        #endregion Methods

        #region Classes

        private class IGMData_HP : IGMData
        {
            #region Fields

            private static Texture2D dot;
            private Mode mode;

            #endregion Fields

            #region Constructors

            public IGMData_HP(Rectangle pos, Characters character, Characters visablecharacter) : base(3, 4, new IGMDataItem_Empty(pos), 1, 3, character, visablecharacter)
            {
            }

            #endregion Constructors

            #region Methods

            public override void ReInit()
            {
                if (Memory.State?.Characters != null)
                {
                    List<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
                    byte pos = (byte)party.FindIndex(x => x.Value == VisableCharacter);
                    foreach (KeyValuePair<int, Characters> pm in party.Where(x => x.Value == VisableCharacter))
                    {
                        Saves.CharacterData c = Memory.State.Characters[Memory.State.PartyData[pm.Key]];
                        FF8String name = Memory.Strings.GetName(pm.Value);
                        int HP = c.CurrentHP(pm.Value);
                        //int MaxHP = c.MaxHP(pm.Value);
                        //float HPpercent = c.PercentFullHP(pm.Value);
                        int CriticalHP = c.CriticalHP(pm.Value);
                        Font.ColorID colorid = Font.ColorID.White;
                        byte palette = 2;
                        if (HP < CriticalHP)
                        {
                            colorid = Font.ColorID.Yellow;
                            palette = 6;
                        }
                        if (HP <= 0)
                        {
                            colorid = Font.ColorID.Red;
                            palette = 5;
                        }
                        byte? fadedpalette = null;
                        Font.ColorID? fadedcolorid = null;
                        if (mode == Mode.YourTurn)
                        {
                            fadedpalette = 7;
                            fadedcolorid = Font.ColorID.Grey;
                            ITEM[pos, 2] = new IGMDataItem_Texture(dot, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), Color.Yellow * .8f, Color.LightYellow);
                        }
                        else if (mode == Mode.ATB_Charged)
                            ITEM[pos, 2] = new IGMDataItem_Texture(dot, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), Color.Yellow * .8f);
                        // insert gradient atb bar here. Though this probably belongs in the update
                        // method as it'll be in constant flux.
                        else ITEM[pos, 2] = null;

                        // TODO: make a font render that can draw right to left from a point. For Right aligning the names.
                        ITEM[pos, 0] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), colorid, faded_color: fadedcolorid);
                        ITEM[pos, 1] = new IGMDataItem_Int(HP, new Rectangle(SIZE[pos].X + 128, SIZE[pos].Y, 0, 0), palette: palette, faded_palette: fadedpalette, spaces: 4, numtype: Icons.NumType.Num_8x16_1);

                        ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.Size_08x64_Bar, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), 0);
                        pos++;
                    }
                    base.ReInit();
                }
            }

            protected override void Init()
            {
                if (dot == null)
                {
                    dot = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                    lock (dot)
                        dot.SetData(new Color[] { Color.White });
                }
                base.Init();
            }
            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(Mode))
                {
                    mode = (Mode)e;
                    ReInit();
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }

    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public class BattleMenus : Menus
    {

        #region Fields

        private int _player = 0;
        private Dictionary<Mode, Action> DrawActions;
        private Dictionary<Mode, Func<bool>> InputFunctions;
        private Dictionary<Mode, Func<bool>> UpdateFunctions;

        #endregion Fields

        //private Mode _mode = Mode.Starting;

        #region Enums

        public enum Mode : byte
        {
            Starting,
            Battle,
            Victory,
            GameOver,
        }

        private enum SectionName : byte
        {
            HP
        }

        #endregion Enums

        #region Methods

        public override void Draw()
        {
            StartDraw();
            DrawData();
            //menus?.ForEach(m => m.DrawData());
            EndDraw();
        }

        public override void DrawData()
        {
            if (DrawActions.ContainsKey((Mode)GetMode()))
                DrawActions[(Mode)GetMode()]();
            base.DrawData();
        }

        public override bool Inputs()
        {
            bool ret = false;
            if (InputFunctions.ContainsKey((Mode)GetMode()))
                ret = InputFunctions[(Mode)GetMode()]() && ret;
            return ret;
        }

        public override void ReInit()
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
                SetMode(Mode.Victory);
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
                    //{Mode.GameOver, DrawGameOverAction},
                };
                InputFunctions = new Dictionary<Mode, Func<bool>>()
                {
                    //{Mode.Starting, InputStartingFunction},
                    {Mode.Battle, InputBattleFunction},
                    //{Mode.Victory, InputVictoryFunction},
                    //{Mode.GameOver, InputGameOverFunction},
                };
                menus?.ForEach(m => m.Show());
            }
            base.ReInit();
        }

        public override bool Update()
        {
            bool ret = false;
            if (UpdateFunctions.ContainsKey((Mode)GetMode()))
                UpdateFunctions[(Mode)GetMode()]();
            ret = base.Update() || ret;
            return ret;
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 881, Y = 636 };
            Data = new Dictionary<Enum, IGMData>()
            {
                //{SectionName.HP, new IGMData_HP(new Rectangle((int)(Size.X-389),507,389,126))}
            };
            base.Init();
        }

        private void DrawBattleAction() => menus?.Where(m=>m.GetType().Equals(typeof(BattleMenu))).ForEach(m => m.Draw());

        private void DrawGameOverAction() {}

        private void DrawStartingAction() { }

        private void DrawVictoryAction() => Victory_Menu.DrawData();

        public VictoryMenu Victory_Menu => (VictoryMenu)(menus?.Where(m => m.GetType().Equals(typeof(VictoryMenu))).First());
        private bool InputBattleFunction()
        {
            bool ret = false;
            foreach (var m in menus.Where(m => (BattleMenu.Mode)m.GetMode() == BattleMenu.Mode.YourTurn))
            {
                ret = m.Inputs() || ret;
                if (ret) return ret;
            }
            if (Input.Button(Buttons.Cancel))
            {
                switch ((BattleMenu.Mode)menus[_player].GetMode())
                {
                    case BattleMenu.Mode.YourTurn:
                        menus[_player].SetMode(BattleMenu.Mode.ATB_Charged);
                        break;
                }
                if (++_player > 2) _player = 0;
                init_debugger_Audio.PlaySound(14);
                menus[_player].SetMode(BattleMenu.Mode.YourTurn);
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
        private bool UpdateBattleFunction()
        {
            menus?[_player].SetMode(BattleMenu.Mode.YourTurn);
            bool ret = false;
            foreach (var m in menus)
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
            return false;
        }

        public class VictoryMenu : Menu
        {
            public enum Mode
            {
                Exp,
                ExpAdded,
                ItemsNone,
                ItemsFound,
                ItemsAdded,
                AP,
                APAdded,
                All,
            }
            protected override void Init()
            {
                Size = new Vector2(881, 606);
                Data = new Dictionary<Enum, IGMData>
                {
                    { Mode.Exp,
                    new IGMData_Group (
                        new IGMData_Container(new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL,30,23),new Rectangle(Point.Zero,new Point((int)Size.X,78)),Icons.ID.INFO)),
                        new IGMData_PlayerEXP(partypos:0),new IGMData_PlayerEXP(partypos:1),new IGMData_PlayerEXP(partypos:2)
                        )
                    { CONTAINER = new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))} },
                    { Mode.All,
                    new IGMData_Container(new IGMDataItem_Box(new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.Green,
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm,
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.White}).Append(Memory.Strings.Read(Strings.FileID.KERNEL,30,22)),new Rectangle(new Point(0,(int)Size.Y-78),new Point((int)Size.X,78))))
                    }
                    
                };
                base.Init();
            }

            public override bool Inputs() => throw new NotImplementedException();

            int _ap = 0;
            int _exp = 0;
            Saves.Item[] _items = null;

            /// <summary>
            /// if you use this you will get no exp, ap, or items
            /// </summary>
            public override void ReInit()
            {
                ReInit(exp: 0, ap: 0);
            }
            /// <summary>
            /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
            /// </summary>
            public override void ReInit(Characters c, Characters vc, bool backup = false)
            {
                ReInit(exp: 0, ap: 0);
            }
            public void ReInit(int exp,int ap, params Saves.Item[] items)
            {
                _exp = exp;
                _ap = ap;
                _items = items;
                base.ReInit();
            }
            /// <summary>
            /// <para>EXP Acquired</para>
            /// <para>Current EXP</para>
            /// <para>Next LEVEL</para>
            /// </summary>
            private static FF8String ECN;
            private class IGMData_PlayerEXP : IGMData
            {

                public IGMData_PlayerEXP(sbyte partypos) : base(1, 4, new IGMDataItem_Box(pos:new Rectangle(35,78+partypos*150,808,150)),1,1,partypos:partypos)
                {
                    Debug.Assert(partypos >= 0 && partypos <= 2);
                }
                protected override void Init()
                {
                    if (Character != Characters.Blank)
                    {
                        if(ECN == null)
                            ECN = Memory.Strings.Read(Strings.FileID.KERNEL, 30, 29) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 30) + "\n" +
                                Memory.Strings.Read(Strings.FileID.KERNEL, 30, 31);
                        uint exp = Memory.State[Character].Experience;
                        ushort expTNL = Memory.State[Character].ExperienceToNextLevel;
                        byte lvl = Memory.State[Character].Level;
                        base.Init();
                    }
                }
                public override bool Update()
                {
                    if (Character != Characters.Blank)
                    {
                        return base.Update();
                    }
                    return false;
                }
            }
        }
        #endregion Methods

    }

    public abstract class Menus : Menu
    {

        #region Fields

        protected List<Menu> menus;

        #endregion Fields

    }
}