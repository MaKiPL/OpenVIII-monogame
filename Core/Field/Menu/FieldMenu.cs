using Microsoft.Xna.Framework;
using System.Linq;

namespace OpenVIII.Fields
{
    /// <summary>
    /// Currently a menu for field screen to enable or disable elements or test scripts.
    /// </summary>
    public class FieldMenu : Menu
    {
        #region Enums

        private enum Mode
        {
            On
        }

        #endregion Enums

        #region Methods

        public static FieldMenu Create() => Create<FieldMenu>();

        public override bool Inputs()
        {
            var r = false;
            switch ((Mode)GetMode())
            {
                case Mode.On:
                    r = Data[GetMode()].Inputs() || r;
                    break;
            }
            r = base.Inputs() || r;

            return r;
        }

        protected override void Init()
        {
            //Size = new Vector2(960f, 720f);
            Size = new Vector2(1280f, 720f);
            base.Init();
            Data[Mode.On] = IGMData.FieldDebugControls.Create(new Rectangle(0, 0, 480, 360));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.On);
        }

        #endregion Methods
    }
}