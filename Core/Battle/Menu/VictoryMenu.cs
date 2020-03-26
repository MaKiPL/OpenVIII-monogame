using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenVIII
{
    public partial class VictoryMenu : Menu
    {
        #region Fields

        private uint _ap = 0;

        private ConcurrentDictionary<Cards.ID, byte> _cards;

        private int _exp = 0;

        private ConcurrentDictionary<Characters, int> _expextra;

        private ConcurrentDictionary<byte, byte> _items;

        private IReadOnlyDictionary<Mode, Func<bool>> InputFunctions;

        #endregion Fields

        #region Enums

        public enum Mode
        {
            Exp,
            Items,
            AP,
            All,
        }

        #endregion Enums

        #region Methods

        public static VictoryMenu Create() => Create<VictoryMenu>();

        public override bool Inputs()
        {
            var ret = false;
            if (InputFunctions != null && InputFunctions.TryGetValue((Mode)GetMode(), out var fun))
            {
                ret = fun();
            }
            if (!ret && Input2.Button(FF8TextTagKey.Confirm))
            {
                do
                {
                    SetMode(((Mode)GetMode()) + 1);
                }
                while (
                (GetMode().Equals(Mode.Items) && _items.Count + _cards.Count == 0) ||
                (GetMode().Equals(Mode.AP) && _ap == 0));
            }
            return true;
        }

        /// <summary>
        /// if you use this you will get no exp, ap, or items
        /// </summary>
        public override void Refresh() { }

        /// <summary>
        /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
        /// </summary>
        public override void Refresh(Damageable damageable, bool backup = false) { }

        public void Refresh(int exp, uint ap, ConcurrentDictionary<Characters, int> expextra, ConcurrentDictionary<byte, byte> items, ConcurrentDictionary<Cards.ID, byte> cards)
        {
            SetMode(Mode.Exp);

            _expextra = expextra;
            _exp = exp;
            ((IGMData.Group.PlayerEXP)Data[Mode.Exp]).NoEarnExp = true;
            ((IGMData.Group.PlayerEXP)Data[Mode.Exp]).EXP = _exp;
            ((IGMData.Group.PlayerEXP)Data[Mode.Exp]).EXPExtra = _expextra;
            ((IGMData.Group.PlayerEXP)Data[Mode.Exp]).NoEarnExp = false;
            _ap = ap;
            ((IGMData.PartyAP)Data[Mode.AP]).AP = _ap;
            _items = items;
            _cards = cards;
            ((IGMData.PartyItems)Data[Mode.Items]).SetItems(_items);
            ((IGMData.PartyItems)Data[Mode.Items]).SetItems(_cards);
            base.Refresh();
        }

        public override bool SetMode(Enum mode)
        {
            if (mode.GetType() == typeof(Mode))
            {
                switch ((Mode)mode)
                {
                    case Mode.AP:
                        Data[Mode.Exp].Hide();
                        Data[Mode.Items].Hide();
                        Data[Mode.AP].Show();
                        Data[Mode.AP].Refresh();
                        break;

                    case Mode.Exp:
                        Data[Mode.Exp].Show();
                        Data[Mode.Items].Hide();
                        Data[Mode.AP].Hide();
                        Data[Mode.Exp].Refresh();
                        break;

                    case Mode.Items:
                        Data[Mode.Exp].Hide();
                        Data[Mode.Items].Show();
                        Data[Mode.AP].Hide();
                        Data[Mode.Items].Refresh();
                        break;

                    default:
                        Menu.BattleMenus.ReturnTo();
                        break;
                }
                return base.SetMode((Mode)mode);
            }
            return false;
        }

        protected override void Init()
        {
            NoInputOnUpdate = true;
            Size = new Vector2(881, 606);
            base.Init();
            var tmp = new Menu_Base[3];

            var actions = new Action[]
            {
                () => Data.TryAdd(Mode.All, IGMData.Group.Base.Create(
                        new IGMDataItem.Box{ Data = new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm})+
                            "  "+
                            (Strings.Name.To_confirm),
                            Pos = new Rectangle(0,(int)Size.Y-78,(int)Size.X,78),Options= Box_Options.Center | Box_Options.Middle })),

                    () => tmp[0] = IGMData.PlayerEXP.Create(0),
                    () => tmp[1] = IGMData.PlayerEXP.Create(1),
                    () => tmp[2] = IGMData.PlayerEXP.Create(2),
                    () => Data.TryAdd(Mode.Items, IGMData.PartyItems.Create(new Rectangle(Point.Zero,Size.ToPoint()))),
                    () => Data.TryAdd(Mode.AP, IGMData.PartyAP.Create(new Rectangle(Point.Zero,Size.ToPoint()))),
            };

            Memory.ProcessActions(actions);

            Data.TryAdd(Mode.Exp, IGMData.Group.PlayerEXP.Create(tmp));
            Data[Mode.Exp].CONTAINER.Pos = new Rectangle(Point.Zero, Size.ToPoint());
            SetMode(Mode.Exp);
            InputFunctions = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.Exp, Data[Mode.Exp].Inputs},
                    { Mode.Items, Data[Mode.Items].Inputs},
                    { Mode.AP, Data[Mode.AP].Inputs}
                };
        }

        #endregion Methods
    }
}