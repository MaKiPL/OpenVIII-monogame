using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_CharacterInfo : IGMData.Base
        {
            #region Constructors

            static public IGMData_CharacterInfo Create() => Create<IGMData_CharacterInfo>(1, 15, new IGMDataItem.Empty(new Rectangle(20, 153, 395, 255)));

            #endregion Constructors

            #region Methods

            /// <summary>
            /// Things that may of changed before screen loads or junction is changed.
            /// </summary>
            public override void Refresh()
            {
                if (Memory.State.Characters != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                {
                    base.Refresh();
                    ((IGMDataItem.Face)ITEM[0, 0]).Data = c.ID.ToFacesID();
                    ((IGMDataItem.Text)ITEM[0, 2]).Data = Damageable.Name;

                    ((IGMDataItem.Integer)ITEM[0, 4]).Data = Damageable.Level;
                    if (Memory.State.Party != null && Memory.State.Party.Contains(c.ID))
                        ITEM[0, 5].Show();
                    else
                        ITEM[0, 5].Hide();
                    ((IGMDataItem.Integer)ITEM[0, 7]).Data = Damageable.CurrentHP();
                    ((IGMDataItem.Integer)ITEM[0, 9]).Data = Damageable.MaxHP();
                    ((IGMDataItem.Integer)ITEM[0, 11]).Data = (int)c.Experience;
                    ((IGMDataItem.Integer)ITEM[0, 13]).Data = c.ExperienceToNextLevel;
                }
            }

            /// <summary>
            /// Things fixed at startup.
            /// </summary>
            protected override void Init()
            {
                //Static items
                ITEM[0, 1] = new IGMDataItem.Icon { Data = Icons.ID.MenuBorder, Pos = new Rectangle(X + 10, Y - 2, 100, 148), Scale = new Vector2(1f) };
                ITEM[0, 3] = new IGMDataItem.Text { Data = Strings.Name.LV, Pos = new Rectangle(X + 117, Y + 54, 0, 0) };
                ITEM[0, 5] = new IGMDataItem.Icon { Data = Icons.ID.InParty, Pos = new Rectangle(X + 278, Y + 48, 0, 0), Palette = 6};
                ITEM[0, 6] = new IGMDataItem.Text { Data = Strings.Name.HP, Pos = new Rectangle(X + 117, Y + 108, 0, 0) };
                ITEM[0, 8] = new IGMDataItem.Text { Data = Strings.Name.ForwardSlash, Pos = new Rectangle(X + 272, Y + 108, 0, 0) };
                ITEM[0, 10] = new IGMDataItem.Text { Data = Strings.Name.CurrentEXP + "\n" + Strings.Name.NextLEVEL, Pos = new Rectangle(X, Y + 192, 0, 0) };
                ITEM[0, 12] = new IGMDataItem.Icon { Data = Icons.ID.P, Pos = new Rectangle(X + 372, Y + 198, 0, 0), Palette = 2 };
                ITEM[0, 14] = new IGMDataItem.Icon { Data = Icons.ID.P, Pos = new Rectangle(X + 372, Y + 231, 0, 0), Palette = 2 };

                //Dynamic items
                ITEM[0, 0] = new IGMDataItem.Face { Pos = new Rectangle(X + 12, Y, 96, 144) };
                ITEM[0, 2] = new IGMDataItem.Text { Pos = new Rectangle(X + 117, Y + 0, 0, 0) };
                ITEM[0, 4] = new IGMDataItem.Integer { Pos = new Rectangle(X + 117 + 35, Y + 54, 0, 0), Palette = 13, NumType= Icons.NumType.sysFntBig, Padding= 1, Spaces= 6 };
                ITEM[0, 7] = new IGMDataItem.Integer { Pos = new Rectangle(X + 152, Y + 108, 0, 0), Palette = 13, NumType= Icons.NumType.sysFntBig, Padding= 1, Spaces= 6 };
                ITEM[0, 9] = new IGMDataItem.Integer { Pos = new Rectangle(X + 292, Y + 108, 0, 0), Palette = 13, NumType= Icons.NumType.sysFntBig, Padding= 1, Spaces= 5 };
                ITEM[0, 11] = new IGMDataItem.Integer { Pos = new Rectangle(X + 192, Y + 198, 0, 0), Palette = 13, NumType= Icons.NumType.Num_8x8_2, Padding= 1, Spaces= 9 };
                ITEM[0, 13] = new IGMDataItem.Integer { Pos = new Rectangle(X + 192, Y + 231, 0, 0), Palette= 13, NumType= Icons.NumType.Num_8x8_2, Padding= 1, Spaces= 9 };
                base.Init();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}