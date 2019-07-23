using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class BattleMenu
    {
        #region Classes

        private class IGMData_HP : IGMData
        {
            #region Fields

            private static Texture2D dot;
            private Mode mode;

            #endregion Fields

            #region Constructors

            public IGMData_HP(Rectangle pos, Characters character, Characters visablecharacter) : base(3, 4, new IGMDataItem_Empty(pos), 1, 3, character, visablecharacter)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                if (Memory.State?.Characters != null)
                {
                    List<KeyValuePair<int, Characters>> party = Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
                    byte pos = (byte)party.FindIndex(x => x.Value == VisableCharacter);
                    foreach (KeyValuePair<int, Characters> pm in party.Where(x => x.Value == VisableCharacter))
                    {
                        Saves.CharacterData c = Memory.State.Characters[Memory.State.PartyData[pm.Key]];
                        FF8String name = Memory.Strings.GetName(pm.Value);
                        int HP = c.CurrentHP(pm.Value);
                        //int MaxHP = c.MaxHP(pm.Value);
                        //float HPpercent = c.PercentFullHP(pm.Value);
                        int CriticalHP = c.CriticalHP(pm.Value);
                        Font.ColorID colorid = Font.ColorID.White;
                        byte palette = 2;
                        if (HP < CriticalHP)
                        {
                            colorid = Font.ColorID.Yellow;
                            palette = 6;
                        }
                        if (HP <= 0)
                        {
                            colorid = Font.ColorID.Red;
                            palette = 5;
                        }
                        byte? fadedpalette = null;
                        Font.ColorID? fadedcolorid = null;
                        bool blink = false;
                        if (mode == Mode.YourTurn)
                        {
                            blink = true;
                            fadedpalette = 7;
                            fadedcolorid = Font.ColorID.Grey;
                            ITEM[pos, 2] = new IGMDataItem_Texture(dot, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), Color.LightYellow * .8f, new Color(125,125,0,255) * .8f) { Blink = blink };
                        }
                        else if (mode == Mode.ATB_Charged)
                            ITEM[pos, 2] = new IGMDataItem_Texture(dot, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), Color.Yellow * .8f);
                        // insert gradient atb bar here. Though this probably belongs in the update
                        // method as it'll be in constant flux.
                        else ITEM[pos, 2] = null;

                        // TODO: make a font render that can draw right to left from a point. For Right aligning the names.
                        ITEM[pos, 0] = new IGMDataItem_String(name, new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0), colorid, faded_fontcolor: fadedcolorid) { Blink = blink };
                        ITEM[pos, 1] = new IGMDataItem_Int(HP, new Rectangle(SIZE[pos].X + 128, SIZE[pos].Y, 0, 0), palette: palette, faded_palette: fadedpalette, spaces: 4, numtype: Icons.NumType.Num_8x16_1) { Blink = blink };

                        ITEM[pos, 3] = new IGMDataItem_Icon(Icons.ID.Size_08x64_Bar, new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15), 0);
                        pos++;
                    }
                    base.Refresh();
                }
            }

            protected override void Init()
            {
                if (dot == null)
                {
                    dot = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                    lock (dot)
                        dot.SetData(new Color[] { Color.White });
                }
                base.Init();
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (e.GetType() == typeof(Mode))
                {
                    mode = (Mode)e;
                    Refresh();
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}