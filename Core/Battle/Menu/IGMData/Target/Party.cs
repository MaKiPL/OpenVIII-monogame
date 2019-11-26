using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData.Target
{
    public class Party : IGMData.Base
    {
        #region Properties

        public IGMData.Target.Enemies Target_Enemies { get; set; }

        #endregion Properties

        #region Methods

        public static Party Create(Rectangle pos) => Create<Party>(3, 1, new IGMDataItem.Box { Pos = pos, Title = Icons.ID.NAME }, 1, 3);

        public override void Inputs_Left()
        {
            Cursor_Status &= ~Cursor_Status.Enabled;
            Target_Enemies.Cursor_Status |= Cursor_Status.Enabled;
            Target_Enemies.CURSOR_SELECT = (CURSOR_SELECT + Target_Enemies.Rows) %
                 (Target_Enemies.Rows * Target_Enemies.Cols);
            while (Target_Enemies.BLANKS[Target_Enemies.CURSOR_SELECT] && Target_Enemies.CURSOR_SELECT > 0)
                Target_Enemies.CURSOR_SELECT--;
            base.Inputs_Left();
        }

        public override bool Inputs_OKAY() => false;

        public override void Inputs_Right()
        {
            Cursor_Status &= ~Cursor_Status.Enabled;
            Target_Enemies.Cursor_Status |= Cursor_Status.Enabled;
            Target_Enemies.CURSOR_SELECT = CURSOR_SELECT % Target_Enemies.Rows;
            while (Target_Enemies.BLANKS[Target_Enemies.CURSOR_SELECT] && Target_Enemies.CURSOR_SELECT > 0)
                Target_Enemies.CURSOR_SELECT--;
            base.Inputs_Right();
        }

        public override void Refresh()
        {
            if (Memory.State?.Characters != null && Memory.State?.Party != null)
            {
                List<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
                byte pos = 0;
                foreach (KeyValuePair<int, Characters> pm in party)
                {
                    Saves.CharacterData data = Memory.State[Memory.State.PartyData[pm.Key]];

                    ((IGMDataItem.Text)ITEM[pos, 0]).Data = data.Name;
                    ((IGMDataItem.Text)ITEM[pos, 0]).FontColor = data.IsDead ? Font.ColorID.Dark_Grey : Font.ColorID.White;

                    BLANKS[pos] = false;

                    ITEM[pos, 0].Show();
                    pos++;
                }
                for (; pos < Count; pos++)
                {
                    BLANKS[pos] = true;
                    ITEM[pos, 0].Hide();
                }
            }
        }

        protected override void Init()
        {
            base.Init();
            for (int pos = 0; pos < Count; pos++)
                ITEM[pos, 0] = new IGMDataItem.Text { Pos = SIZE[pos] };
        }

        protected override void InitShift(int i, int col, int row)
        {
            base.InitShift(i, col, row);
            SIZE[i].Inflate(-18, -20);
            SIZE[i].Y -= 7 * row + 2;
            //SIZE[i].Inflate(-22, -8);
            //SIZE[i].Offset(0, 12 + (-8 * row));
            SIZE[i].Height = (int)(12 * TextScale.Y);
        }

        #endregion Methods
    }
}