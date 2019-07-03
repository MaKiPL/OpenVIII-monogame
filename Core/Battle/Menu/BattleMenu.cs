using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Waiting,
            YourTurn,
            GF_Charging,
        }
        public enum SectionName: byte
        {
            Commands
        }
        #endregion Enums

        #region Methods

        /// <summary>
        /// <para>Draws the IGMData</para>
        /// <para>Skips Start and Stop because this class should be in another class</para>
        /// </summary>
        public override void Draw() => base.DrawData();

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            SetMode((Mode)0);
            Data.Add(SectionName.Commands, new IGMData_Commands(new Rectangle(0, (int)(Size.Y - 192), 210, 192)));
            base.Init();
        }
        protected override bool Inputs() => throw new NotImplementedException();

        #endregion Methods

    }

    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public class BattleMenus : Menus
    {

        //private Mode _mode = Mode.Starting;

        #region Enums

        public enum Mode : byte
        {
            Starting,
            Battle,
            Victory,
            GameOver,
        }

        #endregion Enums

        #region Methods

        public override void Draw()
        {
            StartDraw();
            DrawData();
            menus?.ForEach(m => m.Draw());
            EndDraw();
        }

        public override void ReInit()
        {
            if (Memory.State?.Characters != null)
            {
                IEnumerable<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank));
                int count = party.Count();
                menus = new List<Menu>(count);
                foreach (var m in party)
                {
                    var tmp = new BattleMenu(Memory.State.PartyData[m.Key], m.Value);
                    tmp.Hide();
                    menus.Add(tmp);
                }
            }
            base.ReInit();
        }

        protected override void Init()
        {
            Size = new Vector2 { X = 840, Y = 630 };
            SetMode((Mode)0);
            base.Init();
        }
        protected override bool Inputs() => throw new NotImplementedException();

        #endregion Methods

    }

    public abstract class Menus : Menu
    {

        #region Fields

        protected List<Menu> menus;

        #endregion Fields

    }
}