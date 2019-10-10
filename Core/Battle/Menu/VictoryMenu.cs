using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenVIII
{
    public partial class BattleMenus
    {
        public partial class VictoryMenu : Menu
        {
            public enum Mode
            {
                Exp,
                Items,
                AP,
                All,
            }

            protected override void Init()
            {
                Size = new Vector2(881, 606);
                Data = new Dictionary<Enum, IGMData.Base>
                {
                    { Mode.All, new IGMData.Group.Base(
                    new IGMData_Container(new IGMDataItem.Box(new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm})+
                            " "+
                            (Memory.Strings.Read(Strings.FileID.KERNEL,30,22)),
                            new Rectangle(0,(int)Size.Y-78,(int)Size.X,78),options: Box_Options.Center| Box_Options.Middle))
                    )},
                    { Mode.Exp,
                    new IGMData_PlayerEXPGroup (
                        new IGMData_PlayerEXP(0),new IGMData_PlayerEXP(1),new IGMData_PlayerEXP(2)
                        )
                    { CONTAINER = new IGMDataItem.Empty(new Rectangle(Point.Zero,Size.ToPoint()))} },
                    { Mode.Items,
                    new IGMData_PartyItems(new IGMDataItem.Empty(new Rectangle(Point.Zero,Size.ToPoint()))) },
                    { Mode.AP,
                    new IGMData_PartyAP(new IGMDataItem.Empty(new Rectangle(Point.Zero,Size.ToPoint()))) },

                };
                SetMode(Mode.Exp);
                InputFunctions = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.Exp, Data[Mode.Exp].Inputs},
                    { Mode.Items, Data[Mode.Items].Inputs},
                    { Mode.AP, Data[Mode.AP].Inputs}
                };
                base.Init();
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

            public override bool Inputs()
            {
                bool ret = false;
                if (InputFunctions != null && InputFunctions.TryGetValue((Mode)GetMode(), out Func<bool> fun))
                {
                    ret = fun();
                }
                if(!ret && Input2.Button(FF8TextTagKey.Confirm))
                {
                    SetMode(((Mode)GetMode())+1);
                }
                return true;
            }

            private uint _ap = 0;
            private ConcurrentDictionary<Characters, int> _expextra;
            private int _exp = 0;

            /// <summary>
            /// if you use this you will get no exp, ap, or items
            /// </summary>
            public override void Refresh() { }

            /// <summary>
            /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
            /// </summary>
            public override void Refresh(Damageable damageable, bool backup = false) { }

            public void Refresh(int exp, uint ap, ConcurrentDictionary<Characters,int> expextra, ConcurrentDictionary<byte, byte> items, ConcurrentDictionary<Cards.ID, byte> cards)
            {
                _expextra = expextra;
                _exp = exp;
                ((IGMData_PlayerEXPGroup)Data[Mode.Exp]).EXP = _exp;
                ((IGMData_PlayerEXPGroup)Data[Mode.Exp]).EXPExtra = _expextra;
                _ap = ap;
                ((IGMData_PartyAP)Data[Mode.AP]).AP = _ap;
                _items = items;
                _cards = cards;
                ((IGMData_PartyItems)Data[Mode.Items]).SetItems(_items);
                ((IGMData_PartyItems)Data[Mode.Items]).SetItems(_cards);
                base.Refresh();
            }

            /// <summary>
            /// <para>EXP Acquired</para>
            /// <para>Current EXP</para>
            /// <para>Next LEVEL</para>
            /// </summary>
            private static FF8String ECN;

            private Dictionary<Mode, Func<bool>> InputFunctions;
            private ConcurrentDictionary<byte, byte> _items;
            private ConcurrentDictionary<Cards.ID, byte> _cards;
        }
    }
}