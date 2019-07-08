using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMDataItem_TextureHandler : IGMDataItem
    {
        public TextureHandler Data { get; set; }

        public IGMDataItem_TextureHandler(TextureHandler data, Rectangle? pos = null) : base(pos) => this.Data = data;

        public override void Draw()
        {
            if (Enabled)
            {
                Data.Draw(Pos, null, Color * Fade);//4
            }
        }
    }

}