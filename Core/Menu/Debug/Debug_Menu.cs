using Microsoft.Xna.Framework;
using System.Linq;

namespace OpenVIII
{
    public class Debug_Menu : Menu
    {
        #region Enums

        private enum Mode
        {
            Main
        }

        #endregion Enums

        #region Methods

        public static Debug_Menu Create() => Create<Debug_Menu>();

        public override bool Inputs() => Data[Mode.Main].Inputs() || base.Inputs();

        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            base.Init();
            Data[Mode.Main] = IGMData.DebugChoose.Create(new Rectangle(0, 0, (int)Size.X, (int)Size.Y));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.Main);
        }

        #endregion Methods
    }
}