using Microsoft.Xna.Framework;
using System;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private class IGMData_Abilities_Group : IGMData_Group
        {
            #region Constructors

            public IGMData_Abilities_Group(params IGMData[] d) : base(d)
            {
            }

            #endregion Constructors

            #region Methods

            public override bool Inputs()
            {
                skipdata = true;
                bool ret = base.Inputs();
                skipdata = false;
                IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[3, 0]);
                if (ret && i != null && i.Data != null)
                {
                    if (CURSOR_SELECT >= i.Data.Count)
                        i2.Data.Show();
                    else
                        i2.Data.Hide();
                }
                return ret;
            }

            public override bool Inputs_CANCEL()
            {
                skipdata = true;
                base.Inputs_CANCEL();
                skipdata = false;
                IGM_Junction.Data[SectionName.TopMenu_Abilities].Hide();
                IGM_Junction.SetMode(Mode.TopMenu);
                return true;
            }

            public override bool Inputs_OKAY()
            {
                base.Inputs_OKAY();
                IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[3, 0]);
                if (i != null && i.Data != null)
                {
                    if (CURSOR_SELECT >= i.Data.Count)
                        IGM_Junction.SetMode(Mode.Abilities_Abilities);
                    else
                        IGM_Junction.SetMode(Mode.Abilities_Commands);
                    return true;
                }
                return false;
            }

            public override void Inputs_Menu()
            {
                skipdata = true;
                base.Inputs_Menu();
                skipdata = false;

                IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[3, 0]);
                if (i != null && i.Data != null)
                {
                    if (CURSOR_SELECT < i.Data.Count)
                    {
                        Memory.State.Characters[Character].Commands[CURSOR_SELECT - 1] = Kernel_bin.Abilities.None;
                        IGM_Junction.Data[SectionName.TopMenu_Abilities].Refresh();
                        IGM_Junction.Data[SectionName.Commands].Refresh();
                    }
                    else
                    {
                        Memory.State.Characters[Character].Abilities[CURSOR_SELECT - i.Data.Count] = Kernel_bin.Abilities.None;
                        IGM_Junction.Refresh();
                    }
                }
            }

            public override void Refresh()
            {
                base.Refresh();
                IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[1, 0]);
                if (i != null && i.Data != null && i2 != null && i2.Data != null)
                {
                    SIZE = new Rectangle[i.Data.Count + i2.Data.Count];
                    Array.Copy(i.Data.SIZE, SIZE, i.Data.Count);
                    Array.Copy(i2.Data.SIZE, 0, SIZE, i.Data.Count, i2.Data.Count);
                    CURSOR = new Point[i.Data.Count + i2.Data.Count];
                    Array.Copy(i.Data.CURSOR, CURSOR, i.Data.Count);
                    Array.Copy(i2.Data.CURSOR, 0, CURSOR, i.Data.Count, i2.Data.Count);
                    BLANKS = new bool[i.Data.Count + i2.Data.Count];
                    Array.Copy(i.Data.BLANKS, BLANKS, i.Data.Count);
                    Array.Copy(i2.Data.BLANKS, 0, BLANKS, i.Data.Count, i2.Data.Count);
                }
                if (CURSOR_SELECT == 0)
                    CURSOR_SELECT = 1;
            }

            public override bool Update()
            {
                bool ret = base.Update();

                if (IGM_Junction != null && IGM_Junction.GetMode().Equals(Mode.Abilities))
                {
                    Cursor_Status &= ~Cursor_Status.Blinking;

                    IGMDataItem_IGMData i = ((IGMDataItem_IGMData)ITEM[0, 0]);
                    IGMDataItem_IGMData i2 = ((IGMDataItem_IGMData)ITEM[1, 0]);
                    if (i != null && i.Data != null && i2 != null && i2.Data != null)
                    {
                        if (CURSOR_SELECT >= i.Data.Count)
                        {
                            if (i2.Data.Descriptions != null && i2.Data.Descriptions.ContainsKey(CURSOR_SELECT - i.Data.Count))
                            {
                                IGM_Junction.ChangeHelp(i2.Data.Descriptions[CURSOR_SELECT - i.Data.Count]);
                            }
                        }
                        else
                        {
                            if (i.Data.Descriptions != null && i.Data.Descriptions.ContainsKey(CURSOR_SELECT))
                            {
                                IGM_Junction.ChangeHelp(i.Data.Descriptions[CURSOR_SELECT]);
                            }
                        }
                    }
                }
                else
                    Cursor_Status |= Cursor_Status.Blinking;

                return ret;
            }

            protected override void Init()
            {
                base.Init();
                Cursor_Status |= Cursor_Status.Enabled;
                Hide();
            }

            #endregion Methods
        }

        #endregion Classes
    }
}