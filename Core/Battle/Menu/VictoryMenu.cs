using Microsoft.Xna.Framework;
using OpenVIII.Encoding.Tags;
using System;
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
                Data = new Dictionary<Enum, IGMData>
                {
                    { Mode.All, new IGMData_Group(
                    new IGMData_Container(new IGMDataItem_Box(new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.Green,
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm,
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.White})+" "+(Memory.Strings.Read(Strings.FileID.KERNEL,30,22)),new Rectangle(new Point(0,(int)Size.Y-78),new Point((int)Size.X,78)),options: Box_Options.Center| Box_Options.Middle))
                    )},
                    { Mode.Exp,
                    new IGMData_PlayerEXPGroup (
                        new IGMData_PlayerEXP(_exp,0),new IGMData_PlayerEXP(_exp,1),new IGMData_PlayerEXP(_exp,2)
                        )
                    { CONTAINER = new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))} },
                    { Mode.Items,
                    new IGMData_PartyItems(_items,new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))) },

                };
                SetMode(Mode.Items);
                InputFunctions = new Dictionary<Mode, Func<bool>>
                {
                    { Mode.Exp, Data[Mode.Exp].Inputs},
                    { Mode.Items, Data[Mode.Items].Inputs}
                };
                base.Init();
            }
            public override bool SetMode(Enum mode)
            {
                switch ((Mode)mode)
                {
                    case Mode.Exp:
                        Data[Mode.Exp].Show();
                        Data[Mode.Items].Hide();
                        break;
                    case Mode.Items:
                        Data[Mode.Exp].Hide();
                        Data[Mode.Items].Show();
                        break;
                }
                return base.SetMode((Mode)mode);
            }

            public override bool Inputs()
            {
                if (InputFunctions != null && InputFunctions.ContainsKey((Mode)GetMode()))
                    return InputFunctions[(Mode)GetMode()]();
                return false;
            }

            private int _ap = 0;
            private int _exp = 0;
            private Saves.Item[] _items = null;

            /// <summary>
            /// if you use this you will get no exp, ap, or items
            /// </summary>
            public override void Refresh() { }

            /// <summary>
            /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
            /// </summary>
            public override void Refresh(Characters c, Characters vc, bool backup = false) { }

            public void Refresh(int exp, int ap, params Saves.Item[] items)
            {
                _exp = exp;
                ((IGMData_PlayerEXPGroup)Data[Mode.Exp]).EXP = _exp;
                _ap = ap;
                _items = items;
                ((IGMData_PartyItems)Data[Mode.Items]).Items = _items;
                base.Refresh();
            }

            /// <summary>
            /// <para>EXP Acquired</para>
            /// <para>Current EXP</para>
            /// <para>Next LEVEL</para>
            /// </summary>
            private static FF8String ECN;

            private Dictionary<Mode, Func<bool>> InputFunctions;
        }
    }
}