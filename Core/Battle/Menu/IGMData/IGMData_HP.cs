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

        private class IGMData_HP : IGMData.Base
        {
            #region Fields

            private static Texture2D dot;
            private bool EventAdded = false;

            #endregion Fields

            #region Destructors

            ~IGMData_HP()
            {
                if (EventAdded)
                    RemoveModeChangeEvent(ref Damageable.BattleModeChangeEventHandler);
            }

            #endregion Destructors

            #region Enums

            private enum DepthID : byte
            {
                Name,
                HP,
                ATBCharging,
                ATBCharged,
                ATBBorder
            }

            #endregion Enums

            #region Methods

            private static List<KeyValuePair<int, Characters>> GetParty() => Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();

            private byte GetCharPos() => GetCharPos(GetParty());

            private byte GetCharPos(List<KeyValuePair<int, Characters>> party) => (byte)party.FindIndex(x => Damageable.GetCharacterData(out Saves.CharacterData c) && x.Value == c.ID);

            protected override void Init()
            {
                base.Init();
                EventAdded = true;
                AddModeChangeEvent(ref Damageable.BattleModeChangeEventHandler);
                if (dot == null)
                {
                    dot = new Texture2D(Memory.graphics.GraphicsDevice, 1, 1);
                    lock (dot)
                        dot.SetData(new Color[] { Color.White });
                }
                byte pos = GetCharPos();
                FF8String name = null;
                if (Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                    name = c.Name;

                Rectangle atbbarpos = new Rectangle(SIZE[pos].X + 230, SIZE[pos].Y + 12, 150, 15);
                // TODO: make a font render that can draw right to left from a point. For Right aligning the names.
                ITEM[pos, (byte)DepthID.Name] = new IGMDataItem.Text { Data = name, Pos = new Rectangle(SIZE[pos].X, SIZE[pos].Y, 0, 0) };
                ITEM[pos, (byte)DepthID.HP] = new IGMDataItem.Integer { Pos = new Rectangle(SIZE[pos].X + 128, SIZE[pos].Y, 0, 0), Spaces = 4, NumType = Icons.NumType.Num_8x16_1 };

                ITEM[pos, (byte)DepthID.ATBBorder] = new IGMDataItem.Icon { Data = Icons.ID.Size_08x64_Bar, Pos = atbbarpos, Palette = 0 };
                ITEM[pos, (byte)DepthID.ATBCharged] = new IGMDataItem.Texture { Data = dot, Pos = atbbarpos, Color = Color.LightYellow * .8f, Faded_Color = new Color(125, 125, 0, 255) * .8f };
                ITEM[pos, (byte)DepthID.ATBCharged].Hide();
                ITEM[pos, (int)DepthID.ATBCharging] = IGMDataItem.Gradient.ATB.Create(atbbarpos);
                ((IGMDataItem.Gradient.ATB)ITEM[pos, (byte)DepthID.ATBCharging]).Color = Color.Orange * .8f;
                ((IGMDataItem.Gradient.ATB)ITEM[pos, (byte)DepthID.ATBCharging]).Faded_Color = Color.Orange * .8f;
                ((IGMDataItem.Gradient.ATB)ITEM[pos, (byte)DepthID.ATBCharging]).Refresh(Damageable);
            }

            protected override void ModeChangeEvent(object sender, Enum e)
            {
                base.ModeChangeEvent(sender, e);
                if (!e.Equals(Damageable.BattleMode.EndTurn)) //because endturn triggers BattleMenu refresh.
                    Refresh();
            }

            public static IGMData_HP Create(Rectangle pos, Damageable damageable) => Create<IGMData_HP>(3, 5, new IGMDataItem.Empty(pos), 1, 3, damageable);

            public override void Refresh()
            {
                if (Memory.State?.Characters != null && Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData _c))
                {
                    List<KeyValuePair<int, Characters>> party = GetParty();
                    byte pos = GetCharPos(party);
                    foreach (KeyValuePair<int, Characters> pm in party.Where(x => x.Value == _c.ID))
                    {
                        bool blink = false;
                        bool charging = false;
                        if (Damageable.GetBattleMode().Equals(Damageable.BattleMode.YourTurn))
                        {
                            ((IGMDataItem.Texture)ITEM[pos, (int)DepthID.ATBCharged]).Color = Color.LightYellow * .8f;
                            blink = true;
                        }
                        else if (Damageable.GetBattleMode().Equals(Damageable.BattleMode.ATB_Charged))
                        {
                            ((IGMDataItem.Texture)ITEM[pos, (int)DepthID.ATBCharged]).Color = Color.Yellow * .8f;
                        }
                        else if (Damageable.GetBattleMode().Equals(Damageable.BattleMode.ATB_Charging))
                        {
                            charging = true;
                            ((IGMDataItem.Gradient.ATB)ITEM[pos, (int)DepthID.ATBCharging]).Refresh(Damageable);
                        }
                        ((IGMDataItem.Texture)ITEM[pos, (int)DepthID.ATBCharged]).Blink = blink;
                        if (charging)
                        {
                            ITEM[pos, (int)DepthID.ATBCharged].Hide();
                            ITEM[pos, (int)DepthID.ATBCharging].Show();
                        }
                        else
                        {
                            ITEM[pos, (int)DepthID.ATBCharging].Hide();
                            ITEM[pos, (int)DepthID.ATBCharged].Show();
                        }
                        ((IGMDataItem.Text)ITEM[pos, (byte)DepthID.Name]).Blink = blink;
                        ((IGMDataItem.Integer)ITEM[pos, (byte)DepthID.HP]).Blink = blink;

                        pos++;
                    }
                    base.Refresh();
                }
            }

            public override bool Update()
            {
                List<KeyValuePair<int, Characters>> party = GetParty();
                byte pos = GetCharPos(party);
                if (ITEM[pos, 2].GetType() == typeof(IGMDataItem.Gradient.ATB))
                {
                    IGMDataItem.Gradient.ATB hg = (IGMDataItem.Gradient.ATB)ITEM[pos, 2];
                }
                if (Damageable != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    int HP = c.CurrentHP();
                    int CriticalHP = c.CriticalHP();
                    Font.ColorID colorid = Font.ColorID.White;
                    if (HP < CriticalHP)
                    {
                        colorid = Font.ColorID.Yellow;
                    }
                    if (HP <= 0)
                    {
                        colorid = Font.ColorID.Red;
                    }
                    ((IGMDataItem.Text)ITEM[pos, (byte)DepthID.Name]).FontColor = colorid;
                    ((IGMDataItem.Integer)ITEM[pos, (byte)DepthID.HP]).Data = HP;
                    ((IGMDataItem.Integer)ITEM[pos, (byte)DepthID.HP]).FontColor = colorid;
                }
                return base.Update();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}