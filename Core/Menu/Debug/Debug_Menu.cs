using Microsoft.Xna.Framework;
using System.Linq;

namespace OpenVIII
{
    public class Debug_Menu : Menu
    {
        static public Debug_Menu Create()
        {
            return Create<Debug_Menu>();
        }
        enum Mode {
            Main
        }
        protected override void Init()
        {
            Size = new Vector2 { X = 960, Y = 720 };
            base.Init();
            Data[Mode.Main] = IGMData.DebugChoose.Create(new Rectangle(0,0,(int)Size.X, (int)Size.Y));
            Data.Where(x => x.Value != null).ForEach(x => ModeChangeHandler += x.Value.ModeChangeEvent);
            SetMode(Mode.Main);
        }
        public override bool Inputs()
        {
            return Data[Mode.Main].Inputs() || base.Inputs();
        }
    }
}