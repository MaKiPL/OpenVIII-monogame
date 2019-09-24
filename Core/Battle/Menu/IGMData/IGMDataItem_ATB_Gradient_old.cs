using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_ATB_Gradient_old : IGMDataItem_Renzokeken_Gradient
    {
        public IGMDataItem_ATB_Gradient_old(Rectangle pos, Color? color = null, Rectangle? restriction = null, double time = 0, double delay = 0) : base(pos, color, color, 1f, null, restriction, time, delay,Color.Black,true,false)
        {
        }
    }
}