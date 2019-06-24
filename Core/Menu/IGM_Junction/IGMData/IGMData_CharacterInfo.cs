using Microsoft.Xna.Framework;
using System.Linq;

namespace OpenVIII
{
    public partial class Module_main_menu_debug
    {
        private partial class IGM_Junction
        {
            private class IGMData_CharacterInfo : IGMData
            {
                public IGMData_CharacterInfo() : base( 1, 15, new IGMDataItem_Empty(new Rectangle(20, 153, 395, 255)))
                {
                }

                /// <summary>
                /// Things that may of changed before screen loads or junction is changed.
                /// </summary>
                public override void ReInit()
                {
                    base.ReInit();
                    ITEM[0, 0] = new IGMDataItem_Face((Faces.ID)VisableCharacter, new Rectangle(X + 12, Y, 96, 144));
                    ITEM[0, 2] = new IGMDataItem_String(Memory.Strings.GetName(VisableCharacter), new Rectangle(X + 117, Y + 0, 0, 0));

                    if (Memory.State.Characters != null)
                    {
                        ITEM[0, 4] = new IGMDataItem_Int(Memory.State.Characters[Character].Level, new Rectangle(X + 117 + 35, Y + 54, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 5] = Memory.State.Party != null && Memory.State.Party.Contains(Character)
                            ? new IGMDataItem_Icon(Icons.ID.InParty, new Rectangle(X + 278, Y + 48, 0, 0), 6)
                            : null;
                        ITEM[0, 7] = new IGMDataItem_Int(Memory.State.Characters[Character].CurrentHP(VisableCharacter), new Rectangle(X + 152, Y + 108, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 6);
                        ITEM[0, 9] = new IGMDataItem_Int(Memory.State.Characters[Character].MaxHP(VisableCharacter), new Rectangle(X + 292, Y + 108, 0, 0), 13, numtype: Icons.NumType.sysFntBig, padding: 1, spaces: 5);
                        ITEM[0, 11] = new IGMDataItem_Int((int)Memory.State.Characters[Character].Experience, new Rectangle(X + 192, Y + 198, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                        ITEM[0, 13] = new IGMDataItem_Int(Memory.State.Characters[Character].ExperienceToNextLevel, new Rectangle(X + 192, Y + 231, 0, 0), 13, numtype: Icons.NumType.Num_8x8_2, padding: 1, spaces: 9);
                    }
                }

                /// <summary>
                /// Things fixed at startup.
                /// </summary>
                protected override void Init()
                {
                    ITEM[0, 1] = new IGMDataItem_Icon(Icons.ID.MenuBorder, new Rectangle(X + 10, Y - 2, 100, 148), scale: new Vector2(1f));
                    ITEM[0, 3] = new IGMDataItem_String(Misc[Items.LV], new Rectangle(X + 117, Y + 54, 0, 0));
                    ITEM[0, 6] = new IGMDataItem_String(Misc[Items.HP], new Rectangle(X + 117, Y + 108, 0, 0));
                    ITEM[0, 8] = new IGMDataItem_String(Misc[Items.ForwardSlash], new Rectangle(X + 272, Y + 108, 0, 0));
                    ITEM[0, 10] = new IGMDataItem_String(Misc[Items.CurrentEXP].Append(new FF8String("\n")).Append(Misc[Items.NextLEVEL]), new Rectangle(X, Y + 192, 0, 0));
                    ITEM[0, 12] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 198, 0, 0), 2);
                    ITEM[0, 14] = new IGMDataItem_Icon(Icons.ID.P, new Rectangle(X + 372, Y + 231, 0, 0), 2);
                    base.Init();
                }
            }
        }
    }
}