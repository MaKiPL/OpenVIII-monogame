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
        #region Fields

        //private Mode _mode = Mode.Waiting;

        #endregion Fields

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

        #endregion Enums

        #region Methods

        /// <summary>
        /// <para>Draws the IGMData</para>
        /// <para>Skips Start and Stop because this class should be in another class</para>
        /// </summary>
        public override void Draw() => base.DrawData();

        protected override void Init() => base.Init();
        protected override bool Inputs() => throw new NotImplementedException();

        #endregion Methods
    }

    /// <summary>
    /// Menu holds a menu for each character.
    /// </summary>
    public class BattleMenus : Menus
    {
        #region Fields

        //private Mode _mode = Mode.Starting;

        #endregion Fields

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
                    menus.Add(new BattleMenu(Memory.State.PartyData[m.Key], m.Value));
                }
            }
            base.ReInit();
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