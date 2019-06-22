using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OpenVIII
{
    public static partial class Module_main_menu_debug
    {

        #region Classes

        private partial class IGM_Items
        {

            #region Classes

            /// <summary>
            /// 
            /// </summary>
            /// <remarks>Using Faces.ID because it contains characters and gfs. Can cast to Characters or subtract 16 and cast to GFs</remarks>
            private class IGMData_TargetPool : IGMData_Pool<Saves.Data, Faces.ID>
            {

                #region Fields

                private bool eventSet;

                #endregion Fields

                #region Constructors

                public IGMData_TargetPool() : base(9, 3, new IGMDataItem_Box(pos: new Rectangle(420, 150, 420, 360), title: Icons.ID.NAME), 9, 1)
                {
                    Cursor_Status &= ~Cursor_Status.Enabled;
                }

                #endregion Constructors

                #region Properties

                public Item_In_Menu Item { get; private set; }
                public byte TopMenuChoice { get; private set; }
                bool All => (Item.Target & (Item_In_Menu._Target.All | Item_In_Menu._Target.All2)) != 0;

                bool IsMe => InGameMenu_Items.GetMode() == Mode.UseItemOnTarget;

                #endregion Properties

                #region Methods

                public override void Draw()
                {
                    base.Draw();
                    if (All && IsMe)
                    {
                        // if all draw blinking pointers on everyone.
                        byte i = 0;
                        foreach (var c in CURSOR)
                        {
                            if (!BLANKS[i++])
                                DrawPointer(c, blink: true);
                        }
                    }
                }

                public override bool Inputs()
                {

                    //                    if ((Item.Target & (Item_In_Menu._Target.All|Item_In_Menu._Target.All2)) == 0)
                    Cursor_Status |= Cursor_Status.Enabled;
                    return base.Inputs();
                }

                public override void Inputs_CANCEL()
                {
                    base.Inputs_CANCEL();
                    InGameMenu_Items.SetMode(Mode.SelectItem);
                }

                public override void ReInit()
                {
                    if (!eventSet && InGameMenu_Items != null)
                    {
                        InGameMenu_Items.ModeChangeHandler += ModeChangeEvent;
                        InGameMenu_Items.ChoiceChangeHandler += ChoiceChangeEvent;
                        InGameMenu_Items.ItemChangeHandler += ItemTypeChangeEvent;
                        eventSet = true;
                    }
                }

                private void ChoiceChangeEvent(object sender, KeyValuePair<byte, FF8String> e)
                {
                    TopMenuChoice = e.Key;
                }

                private void ItemTypeChangeEvent(object sender, KeyValuePair<Item_In_Menu, FF8String> e)
                {
                    Item = e.Key;
                }

                private void ModeChangeEvent(object sender, Mode e)
                {
                    if(!IsMe)
                        Cursor_Status &= ~Cursor_Status.Enabled;
                }

                #endregion Methods

            }

            #endregion Classes

        }

        #endregion Classes

    }
}