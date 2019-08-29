using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public class IGMData_Draw_Pool : IGMData_Pool<Saves.CharacterData, byte>
    {
        private bool Battle = true;
        private bool skipReinit = false;

        public IGMData_Draw_Pool(Rectangle pos, Characters character = Characters.Blank, Characters? visablecharacter = null, bool battle = false) : base(5, 3, new IGMDataItem_Box(pos: pos, title: Icons.ID.MAGIC), 4, 13, character, visablecharacter)
        {
            Battle = battle;
            skipReinit = true;
            Refresh();
        }

        public IGMData_Draw_Pool() : base(6, 3, new IGMDataItem_Box(pos: new Rectangle(135, 150, 300, 192), title: Icons.ID.MAGIC), 4, 13)
        {
        }

        
    }
}