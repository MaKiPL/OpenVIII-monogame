using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenVIII.IGMDataItem;
using OpenVIII.IGMDataItem.Gradient;
using System;
using System.Collections.Generic;
using System.Linq;
using Texture = OpenVIII.IGMDataItem.Texture;

namespace OpenVIII.IGMData
{
    public class NamesHPATB : Base
    {
        #region Fields

        private const float ATBAlpha = .8f;
        private const int ATBWidth = 150;
        private static Texture2D _dot;
        private Damageable.BattleMode _battleMode = Damageable.BattleMode.EndTurn;
        private bool _eventAdded;

        #endregion Fields

        #region Destructors

        ~NamesHPATB()
        {
            if (_eventAdded)
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

        public static NamesHPATB Create(Rectangle pos, Damageable damageable) => Create<NamesHPATB>(1, (int)DepthID.Max, new Empty { Pos = pos }, 1, 1, damageable);

        public override void ModeChangeEvent(object sender, Enum e)
        {
            if (!e.Equals(_battleMode))
            {
                base.ModeChangeEvent(sender, e);
                _battleMode = (Damageable.BattleMode)e;
                if (!e.Equals(Damageable.BattleMode.EndTurn)) //because endturn triggers BattleMenu refresh.
                    Refresh();
            }
        }

        public override void Refresh(Damageable damageable)
        {
            if (_eventAdded && damageable != Damageable)
            {
                _eventAdded = false;
                if (Damageable != null)
                    Damageable.BattleModeChangeEventHandler -= ModeChangeEvent;
            }
            base.Refresh(damageable);
        }

        public override void Refresh()
        {
            if (Damageable == null) return;
            if (Memory.State?.Characters != null && Damageable.GetCharacterData(out _))
            {
                var party = GetParty();
                if (GetCharPos(party) == 0xFF) return;
            }

            var pos = PartyPos;
            var rectangle = SIZE[0];
            rectangle.Offset(0f, SIZE[0].Height * pos);
            var atbBarPos = new Rectangle(rectangle.X + 230, rectangle.Y + 12, ATBWidth, 15);
            ((ATB)ITEM[0, (int)DepthID.ATBCharging]).Pos = atbBarPos;
            ((Texture)ITEM[0, (byte)DepthID.ATBCharged]).Pos = atbBarPos;
            ((Icon)ITEM[0, (byte)DepthID.ATBBorder]).Pos = atbBarPos;
            ((GF)ITEM[0, (byte)DepthID.GFCharging]).Pos = atbBarPos;
            ((Text)ITEM[0, (byte)DepthID.Name]).Data = Damageable.Name;
            ((Text)ITEM[0, (byte)DepthID.Name]).Pos = new Rectangle(rectangle.X - 60, rectangle.Y, 0, 0);
            ((Integer)ITEM[0, (byte)DepthID.HP]).Pos = new Rectangle(rectangle.X + 128, rectangle.Y, 0, 0);

            ((Text)ITEM[0, (byte)DepthID.Name]).Draw(true);
            ((Integer)ITEM[0, (byte)DepthID.HP]).Draw(true);

            ((Box)ITEM[0, (byte)DepthID.GFHPBox]).Pos = Rectangle.Union(
                ((Text)ITEM[0, (byte)DepthID.Name]).DataSize,
                ((Integer)ITEM[0, (byte)DepthID.HP]).DataSize);
            ((Box)ITEM[0, (byte)DepthID.GFHPBox]).Y -= 4;
            ((Box)ITEM[0, (byte)DepthID.GFNameBox]).Y = ((Box)ITEM[0, (byte)DepthID.GFHPBox]).Y;
            ((Box)ITEM[0, (byte)DepthID.GFNameBox]).Height = ((Box)ITEM[0, (byte)DepthID.GFHPBox]).Height = rectangle.Height;
            ((Box)ITEM[0, (byte)DepthID.GFNameBox]).X =
                ((Box)ITEM[0, (byte)DepthID.GFHPBox]).X -
                (((Box)ITEM[0, (byte)DepthID.GFHPBox]).Width * 3) / 8;
            ((Box)ITEM[0, (byte)DepthID.GFNameBox]).Width = (((Box)ITEM[0, (byte)DepthID.GFHPBox]).Width * 7) / 8;
            if (Damageable.SummonedGF != null)
            {
                ((Box)ITEM[0, (byte)DepthID.GFNameBox]).Data = Damageable.SummonedGF.Name;
                ((Box)ITEM[0, (byte)DepthID.GFHPBox]).Data = $"{Damageable.SummonedGF.CurrentHP()}";
            }
            if (_eventAdded == false)
            {
                _eventAdded = true;
                Damageable.BattleModeChangeEventHandler += ModeChangeEvent;
            }
            var blink = false;
            var charging = false;
            _battleMode = (Damageable.BattleMode)Damageable.GetBattleMode();

            ITEM[0, (byte)DepthID.HP].Show();
            ITEM[0, (byte)DepthID.Name].Show();
            ITEM[0, (byte)DepthID.GFNameBox].Hide();
            ITEM[0, (byte)DepthID.GFHPBox].Hide();

            ITEM[0, (int)DepthID.ATBCharged].Hide();
            ITEM[0, (int)DepthID.GFCharging].Hide();
            ITEM[0, (int)DepthID.ATBCharging].Hide();
            switch (_battleMode)
            {
                case Damageable.BattleMode.YourTurn:
                    ((Texture)ITEM[0, (int)DepthID.ATBCharged]).Color = Color.LightYellow * ATBAlpha;
                    blink = true;
                    break;

                case Damageable.BattleMode.ATB_Charged:
                    ((Texture)ITEM[0, (int)DepthID.ATBCharged]).Color = Color.Yellow * ATBAlpha;
                    break;

                case Damageable.BattleMode.ATB_Charging:
                    charging = true;
                    ((ATB)ITEM[0, (int)DepthID.ATBCharging]).Refresh(Damageable);
                    ITEM[0, (int)DepthID.ATBCharging].Show();
                    break;

                case Damageable.BattleMode.GF_Charging:
                    charging = true;
                    ITEM[0, (byte)DepthID.HP].Hide();
                    ITEM[0, (byte)DepthID.Name].Hide();
                    ITEM[0, (byte)DepthID.GFNameBox].Show();
                    ITEM[0, (byte)DepthID.GFHPBox].Show();
                    ITEM[0, (int)DepthID.GFCharging].Show();
                    ITEM[0, (int)DepthID.GFCharging].Refresh(Damageable.SummonedGF);
                    break;

                case Damageable.BattleMode.EndTurn:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (!charging)
            {
                ITEM[0, (int)DepthID.ATBCharged].Show();
            }
            ((Texture)ITEM[0, (int)DepthID.ATBCharged]).Blink = blink;
            ((Text)ITEM[0, (byte)DepthID.Name]).Blink = blink;
            ((Integer)ITEM[0, (byte)DepthID.HP]).Blink = blink;

            base.Refresh();
        }

        public void ThreadUnsafeOperations()
        {
            if (_dot == null)
            {
                if (Memory.IsMainThread)
                {
                    var texture2D = new Texture2D(Memory.Graphics.GraphicsDevice, 4, 4);
                    var tmp = new Color[texture2D.Height * texture2D.Width];
                    for (var i = 0; i < tmp.Length; i++)
                        tmp[i] = Color.White;
                    texture2D.SetData(tmp);
                    _dot = texture2D;
                    ATB.ThreadUnsafeOperations(ATBWidth);
                }
                else throw new Exception("Must be in main thread!");
            }
            if (_dot != null)
                ((Texture)(ITEM[0, (byte)DepthID.ATBCharged])).
                    Data = _dot;
        }

        public override bool Update()
        {
            if (ITEM[0, 2] is ATB)
            {
                // unsure if need
            }

            if (Damageable == null) return base.Update();
            int hp = Damageable.CurrentHP();
            int criticalHP = Damageable.CriticalHP();
            var colorID = Font.ColorID.White;
            if (hp < criticalHP)
            {
                colorID = Font.ColorID.Yellow;
            }
            if (hp <= 0)
            {
                colorID = Font.ColorID.Red;
            }
            ((Text)ITEM[0, (byte)DepthID.Name]).FontColor = colorID;
            ((Integer)ITEM[0, (byte)DepthID.HP]).Data = hp;
            ((Integer)ITEM[0, (byte)DepthID.HP]).FontColor = colorID;
            return base.Update();
        }

        protected override void Init()
        {
            base.Init();
            Memory.MainThreadOnlyActions.Enqueue(ThreadUnsafeOperations);

            // TODO: make a font render that can draw right to left from a point. For Right aligning the names.
            var atbBarPos = new Rectangle(SIZE[0].X + 230, SIZE[0].Y + 12, ATBWidth, 15);
            ITEM[0, (byte)DepthID.Name] = new Text();
            ITEM[0, (byte)DepthID.HP] = new Integer { Spaces = 4, NumType = Icons.NumType.Num8X16A };
            ITEM[0, (byte)DepthID.GFHPBox] = new Box { Options = Box_Options.Right | Box_Options.Middle };
            ITEM[0, (byte)DepthID.GFHPBox].Hide();
            ITEM[0, (byte)DepthID.GFNameBox] = new Box { Options = Box_Options.Center | Box_Options.Middle };
            ITEM[0, (byte)DepthID.GFNameBox].Hide();
            ITEM[0, (byte)DepthID.ATBBorder] = new Icon { Data = Icons.ID.Size_08x64_Bar, Palette = 0 };
            ITEM[0, (byte)DepthID.ATBCharged] = new Texture { Color = Color.LightYellow * ATBAlpha, Faded_Color = new Color(125, 125, 0, 255) * ATBAlpha };
            ITEM[0, (byte)DepthID.ATBCharged].Hide();
            ITEM[0, (int)DepthID.ATBCharging] = ATB.Create(atbBarPos);
            ITEM[0, (int)DepthID.GFCharging] = GF.Create(atbBarPos);
            ((ATB)ITEM[0, (byte)DepthID.ATBCharging]).Color = Color.Orange * ATBAlpha;
            ((ATB)ITEM[0, (byte)DepthID.ATBCharging]).Faded_Color = Color.Orange * ATBAlpha;
            ((ATB)ITEM[0, (byte)DepthID.ATBCharging]).Refresh(Damageable);
        }

        private static List<KeyValuePair<int, Characters>> GetParty()
        {
            if (Memory.State != null && Memory.State.Characters)
                return Memory.State.Party.Select((element, index) => new { element, index }).ToDictionary(m => m.index, m => m.element).Where(m => !m.Value.Equals(Characters.Blank)).ToList();
            return null;
        }

        private byte GetCharPos(List<KeyValuePair<int, Characters>> party)
        {
            int i;
            if (party != null && (i = party.FindIndex(x => Damageable.GetCharacterData(out var c) && x.Value == c.ID)) > -1)
                return checked((byte)i);
            return 0xFF;
        }

        #endregion Methods
    }
}