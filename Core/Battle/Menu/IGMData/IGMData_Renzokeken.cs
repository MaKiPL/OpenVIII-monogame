using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenVIII
{
    public class IGMData_Renzokeken : IGMData
    {
        #region Methods

        protected override void Init()
        {
            Texture2D pixel = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
            pixel.SetData(new Color[] { Color.White });
            int dark = 12;
            int fade = 180;
            Color lightline = new Color(118, 118, 118, 255);
            Color darkline = new Color(58, 58, 58, 255);
            Color[] cfade = new Color[12 + 180];
            int pos;
            for (pos = 0; pos < dark; pos++)
                cfade[pos] = darkline;
            for (; pos < cfade.Length; pos++)
                cfade[pos] = Color.Lerp(lightline, Color.Black, (float)(pos - dark) / (fade));
            Texture2D line = new Texture2D(Memory.graphics.GraphicsDevice, cfade.Length, 1);
            line.SetData(cfade);
            Memory.Icons.Trim(Icons.ID.Text_Cursor, 6);            
            EntryGroup split = Memory.Icons[Icons.ID.Text_Cursor];
            Rectangle r = new Rectangle(39, 525, 882, 84);
            Color rc = Memory.Icons.AverageColor(Icons.ID.Text_Cursor, 6);
            ITEM[0, 0] = new IGMDataItem_Texture(pixel, r, rc);
            r.Inflate(-6, -6);
            ITEM[1, 0] = new IGMDataItem_Texture(pixel, r, Color.Black);
            Reset();
            base.Init();
            Show();
        }

        #endregion Methods

        #region Constructors

        public IGMData_Renzokeken() : base(5, 1, new IGMDataItem_Box(pos: new Rectangle(24, 501, 912, 123), title: Icons.ID.SPECIAL), 0, 0, Characters.Squall_Leonhart)
        {
            
        }
        public override void Reset()
        {
            //Hide();
            base.Reset();
        }
        #endregion Constructors
    }
}