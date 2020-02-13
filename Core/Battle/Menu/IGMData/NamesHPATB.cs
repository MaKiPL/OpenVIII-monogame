using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII.IGMData
{
    public class NamesHPATB : IGMData.Base
    {
        #region Fields

        private const float ATBalpha = .8f;
        private const int ATBWidth = 150;
        private static Texture2D dot;
        private Damageable.BattleMode BattleMode = Damageable.BattleMode.EndTurn;
        private bool EventAdded = false;

        #endregion Fields

        #region Destructors

        ~NamesHPATB()
        {
            if (EventAdded)
                Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
        }

        #endregion Destructors

        #region Enums

        private enum DepthID : byte
        {
            /// <summary>
            /// Name
            /// </summary>
            Name,

            /// <summary>
            /// HP
            /// </summary>
            HP,

            /// <summary>
            /// Box with GF HP
            /// </summary>
            GFHPBox,

            /// <summary>
            /// Box with GF name
            /// </summary>
            GFNameBox,

            /// <summary>
            /// ATB charging orange red or dark blue (haste/slowed)
            /// </summary>
            ATBCharging,

            /// <summary>
            /// ATB charged blink yellow
            /// </summary>
            ATBCharged,

            /// <summary>
            /// blue white gradient that decreases as the gf is charging.
            /// </summary>
            GFCharging,

            /// <summary>
            /// border around ATB bar
            /// </summary>
            ATBBorder,

            /// <summary>
            /// Max?
            /// </summary>
            Max
        }

        #endregion Enums

        #region Methods

        public static NamesHPATB Create(Rectangle pos, Damageable damageable) => Create<NamesHPATB>(1, (int)DepthID.Max, new IGMDataItem.Empty { Pos = pos }, 1, 1, damageable);

        public void ThreadUnsafeOperations()
        {
            if (dot == null)
            {
                if (Memory.IsMainThread)
                {
                    Texture2D localdot = new Texture2D(Memory.graphics.GraphicsDevice, 4, 4);
                    Color[] tmp = new Color[localdot.Height * localdot.Width];
                    for (int i = 0; i < tmp.Length; i++)
                        tmp[i] = Color.White;
                    localdot.SetData(tmp);
                    dot = localdot;
                    IGMDataItem.Gradient.ATB.ThreadUnsafeOperations(ATBWidth);
                }
                else throw new Exception("Must be in main thread!");
            }
            if (dot != null)
                ((IGMDataItem.Texture)(ITEM[0, (byte)DepthID.ATBCharged])).
                    Data = dot;
        }

        public override void ModeChangeEvent(object sender, Enum e)
        {
            if (!e.Equals(BattleMode))
            {
                base.ModeChangeEvent(sender, e);
                BattleMode = (Damageable.BattleMode)e;
                if (!e.Equals(Damageable.BattleMode.EndTurn)) //because endturn triggers BattleMenu refresh.
                    Refresh();
            }
        }

        public override void Refresh(Damageable damageable)
        {
            if (EventAdded && damageable != Damageable)
            {
                EventAdded = false;
                if (Damageable != null)
                    Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
            }
            base.Refresh(damageable);
        }

        public override void Refresh()
        {
            if (Damageable != null)
            {
                if (Memory.State?.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    List<KeyValuePair<int, Characters>> party = GetParty();
                    if (GetCharPos(party) == 0xFF) return;
                }
                else
                {
                }
                sbyte pos = PartyPos;
                Rectangle rectangle = SIZE[0];
                rectangle.Offset(0f, SIZE[0].Height * pos);
                Rectangle atbbarpos = new Rectangle(rectangle.X + 230, rectangle.Y + 12, ATBWidth, 15);
                ((IGMDataItem.Gradient.ATB)ITEM[0, (int)DepthID.ATBCharging]).Pos = atbbarpos;
                ((IGMDataItem.Texture)ITEM[0, (byte)DepthID.ATBCharged]).Pos = atbbarpos;
                ((IGMDataItem.Icon)ITEM[0, (byte)DepthID.ATBBorder]).Pos = atbbarpos;
                ((IGMDataItem.Gradient.GF)ITEM[0, (byte)DepthID.GFCharging]).Pos = atbbarpos;
                ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).Data = Damageable.Name;
                ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).Pos = new Rectangle(rectangle.X - 60, rectangle.Y, 0, 0);
                ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).Pos = new Rectangle(rectangle.X + 128, rectangle.Y, 0, 0);

                ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).Draw(true);
                ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).Draw(true);

                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Pos = Rectangle.Union(
                    ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).DataSize,
                    ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).DataSize);
                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Y -= 4;
                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFNameBox]).Y = ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Y;
                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFNameBox]).Height = ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Height = rectangle.Height;
                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFNameBox]).X =
                    ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).X -
                    (((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Width * 3) / 8;
                ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFNameBox]).Width = (((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Width * 7) / 8;
                if (Damageable.SummonedGF != null)
                {
                    ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFNameBox]).Data = Damageable.SummonedGF.Name;
                    ((IGMDataItem.Box)ITEM[0, (byte)DepthID.GFHPBox]).Data = $"{Damageable.SummonedGF.CurrentHP()}";
                }
                if (EventAdded == false)
                {
                    EventAdded = true;
                    Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
                }
                bool blink = false;
                bool charging = false;
                BattleMode = (Damageable.BattleMode)Damageable.GetBattleMode();

                ITEM[0, (byte)DepthID.HP].Show();
                ITEM[0, (byte)DepthID.Name].Show();
                ITEM[0, (byte)DepthID.GFNameBox].Hide();
                ITEM[0, (byte)DepthID.GFHPBox].Hide();

                ITEM[0, (int)DepthID.ATBCharged].Hide();
                ITEM[0, (int)DepthID.GFCharging].Hide();
                ITEM[0, (int)DepthID.ATBCharging].Hide();
                if (BattleMode.Equals(Damageable.BattleMode.YourTurn))
                {
                    ((IGMDataItem.Texture)ITEM[0, (int)DepthID.ATBCharged]).Color = Color.LightYellow * ATBalpha;
                    blink = true;
                }
                else if (BattleMode.Equals(Damageable.BattleMode.ATB_Charged))
                {
                    ((IGMDataItem.Texture)ITEM[0, (int)DepthID.ATBCharged]).Color = Color.Yellow * ATBalpha;
                }
                else if (BattleMode.Equals(Damageable.BattleMode.ATB_Charging))
                {
                    charging = true;
                    ((IGMDataItem.Gradient.ATB)ITEM[0, (int)DepthID.ATBCharging]).Refresh(Damageable);
                    ITEM[0, (int)DepthID.ATBCharging].Show();
                }
                else if (BattleMode.Equals(Damageable.BattleMode.GF_Charging))
                {
                    charging = true;
                    ITEM[0, (byte)DepthID.HP].Hide();
                    ITEM[0, (byte)DepthID.Name].Hide();
                    ITEM[0, (byte)DepthID.GFNameBox].Show();
                    ITEM[0, (byte)DepthID.GFHPBox].Show();
                    ITEM[0, (int)DepthID.GFCharging].Show();
                    ITEM[0, (int)DepthID.GFCharging].Refresh(Damageable.SummonedGF);
                }
                if (!charging)
                {
                    ITEM[0, (int)DepthID.ATBCharged].Show();
                }
                ((IGMDataItem.Texture)ITEM[0, (int)DepthID.ATBCharged]).Blink = blink;
                ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).Blink = blink;
                ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).Blink = blink;

                base.Refresh();
            }
        }

        public override bool Update()
        {
            if (ITEM[0, 2].GetType() == typeof(IGMDataItem.Gradient.ATB))
            {
                IGMDataItem.Gradient.ATB hg = (IGMDataItem.Gradient.ATB)ITEM[0, 2];
            }
            if (Damageable != null)
            {
                int HP = Damageable.CurrentHP();
                int CriticalHP = Damageable.CriticalHP();
                Font.ColorID colorid = Font.ColorID.White;
                if (HP < CriticalHP)
                {
                    colorid = Font.ColorID.Yellow;
                }
                if (HP <= 0)
                {
                    colorid = Font.ColorID.Red;
                }
                ((IGMDataItem.Text)ITEM[0, (byte)DepthID.Name]).FontColor = colorid;
                ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).Data = HP;
                ((IGMDataItem.Integer)ITEM[0, (byte)DepthID.HP]).FontColor = colorid;
            }
            return base.Update();
        }

        protected override void Init()
        {
            base.Init();
            Memory.MainThreadOnlyActions.Enqueue(ThreadUnsafeOperations);

            // TODO: make a font render that can draw right to left from a point. For Right aligning the names.
            Rectangle atbbarpos = new Rectangle(SIZE[0].X + 230, SIZE[0].Y + 12, ATBWidth, 15);
            ITEM[0, (byte)DepthID.Name] = new IGMDataItem.Text { };
            ITEM[0, (byte)DepthID.HP] = new IGMDataItem.Integer { Spaces = 4, NumType = Icons.NumType.Num_8x16_1 };
            ITEM[0, (byte)DepthID.GFHPBox] = new IGMDataItem.Box { Options = Box_Options.Right | Box_Options.Middle };
            ITEM[0, (byte)DepthID.GFHPBox].Hide();
            ITEM[0, (byte)DepthID.GFNameBox] = new IGMDataItem.Box { Options = Box_Options.Center | Box_Options.Middle };
            ITEM[0, (byte)DepthID.GFNameBox].Hide();
            ITEM[0, (byte)DepthID.ATBBorder] = new IGMDataItem.Icon { Data = Icons.ID.Size_08x64_Bar, Palette = 0 };
            ITEM[0, (byte)DepthID.ATBCharged] = new IGMDataItem.Texture { Color = Color.LightYellow * ATBalpha, Faded_Color = new Color(125, 125, 0, 255) * ATBalpha };
            ITEM[0, (byte)DepthID.ATBCharged].Hide();
            ITEM[0, (int)DepthID.ATBCharging] = IGMDataItem.Gradient.ATB.Create(atbbarpos);
            ITEM[0, (int)DepthID.GFCharging] = IGMDataItem.Gradient.GF.Create(atbbarpos);
            ((IGMDataItem.Gradient.ATB)ITEM[0, (byte)DepthID.ATBCharging]).Color = Color.Orange * ATBalpha;
            ((IGMDataItem.Gradient.ATB)ITEM[0, (byte)DepthID.ATBCharging]).Faded_Color = Color.Orange * ATBalpha;
            ((IGMDataItem.Gradient.ATB)ITEM[0, (byte)DepthID.ATBCharging]).Refresh(Damageable);
        }

        private static List<KeyValuePair<int, Characters>> GetParty()
        {
            if (Memory.State != null && Memory.State.Characters != null)
                return Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
            return null;
        }

        private byte GetCharPos() => GetCharPos(GetParty());

        private byte GetCharPos(List<KeyValuePair<int, Characters>> party)
        {
            int i = -1;
            if (party != null && (i = party.FindIndex(x => Damageable.GetCharacterData(out Saves.CharacterData c) && x.Value == c.ID)) > -1)
                return checked((byte)i);
            return 0xFF;
        }

        #endregion Methods
    }
}