using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using OpenVIII.Encoding.Tags;

namespace OpenVIII
{

    public partial class BattleMenus
    {
        public partial class VictoryMenu : Menu
        {
            public enum Mode
            {
                Exp,
                ExpAdded,
                ItemsNone,
                ItemsFound,
                ItemsAdded,
                AP,
                APAdded,
                All,
            }
            protected override void Init()
            {
                Size = new Vector2(881, 606);
                Data = new Dictionary<Enum, IGMData>
                {
                    { Mode.Exp,
                    new IGMData_Group (
                        new IGMData_Container(new IGMDataItem_Box(Memory.Strings.Read(Strings.FileID.KERNEL,30,23),new Rectangle(Point.Zero,new Point((int)Size.X,78)),Icons.ID.INFO)),
                        new IGMData_PlayerEXP(partypos:0),new IGMData_PlayerEXP(partypos:1),new IGMData_PlayerEXP(partypos:2)
                        )
                    { CONTAINER = new IGMDataItem_Empty(new Rectangle(Point.Zero,Size.ToPoint()))} },
                    { Mode.All,
                    new IGMData_Container(new IGMDataItem_Box(new FF8String(new byte[] {
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.Green,
                            (byte)FF8TextTagCode.Key,
                            (byte)FF8TextTagKey.Confirm,
                            (byte)FF8TextTagCode.Color,
                            (byte)FF8TextTagColor.White}).Append(Memory.Strings.Read(Strings.FileID.KERNEL,30,22)),new Rectangle(new Point(0,(int)Size.Y-78),new Point((int)Size.X,78))))
                    }
                    
                };
                base.Init();
            }

            public override bool Inputs() => throw new NotImplementedException();

            int _ap = 0;
            int _exp = 0;
            Saves.Item[] _items = null;

            /// <summary>
            /// if you use this you will get no exp, ap, or items
            /// </summary>
            public override void ReInit()
            {
                ReInit(exp: 0, ap: 0);
            }
            /// <summary>
            /// if you use this you will get no exp, ap, or items, No character specifics for this menu.
            /// </summary>
            public override void ReInit(Characters c, Characters vc, bool backup = false)
            {
                ReInit(exp: 0, ap: 0);
            }
            public void ReInit(int exp,int ap, params Saves.Item[] items)
            {
                _exp = exp;
                _ap = ap;
                _items = items;
                base.ReInit();
            }
            /// <summary>
            /// <para>EXP Acquired</para>
            /// <para>Current EXP</para>
            /// <para>Next LEVEL</para>
            /// </summary>
            private static FF8String ECN;
        }
        #endregion Methods

    }
}