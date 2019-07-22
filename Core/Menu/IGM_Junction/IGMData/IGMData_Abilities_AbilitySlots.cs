using Microsoft.Xna.Framework;

namespace OpenVIII
{
        public partial class IGM_Junction
        {
            private class IGMData_Abilities_AbilitySlots : IGMData
            {
                public IGMData_Abilities_AbilitySlots() : base( 4, 2, new IGMDataItem_Box(pos: new Rectangle(0, 414, 435, 216), title: Icons.ID.ABILITY), 1, 4)
                {
                }

                protected override void InitShift(int i, int col, int row)
                {
                    base.InitShift(i, col, row);
                    SIZE[i].Inflate(-22, -8);
                    SIZE[i].Offset(80, 12 + (-8 * row));
                    CURSOR[i].X += 40;
                }

                public override void Refresh()
                {
                    base.Refresh();

                    if (Memory.State.Characters != null && Character != Characters.Blank)
                    {
                        for (int i = 0; i < Count; i++)
                        {
                            int slots = 2;
                            if (Memory.State.Characters[Character].UnlockedGFAbilities.Contains(Kernel_bin.Abilities.Abilityx3))
                                slots = 3;
                            if (Memory.State.Characters[Character].UnlockedGFAbilities.Contains(Kernel_bin.Abilities.Abilityx4))
                                slots = 4;
                            if (i < slots)
                            {
                                ITEM[i, 0] = new IGMDataItem_Icon(Icons.ID.Arrow_Right2, SIZE[i], 9);
                                if (Memory.State.Characters[Character].Abilities[i] != Kernel_bin.Abilities.None)
                                {
                                    ITEM[i, 1] = new IGMDataItem_String(

                                    Kernel_bin.EquipableAbilities[Memory.State.Characters[Character].Abilities[i]].Icon, 9,
                                    Kernel_bin.EquipableAbilities[Memory.State.Characters[Character].Abilities[i]].Name,
                                    new Rectangle(SIZE[i].X + 40, SIZE[i].Y, 0, 0));
                                    Descriptions[i] = Kernel_bin.EquipableAbilities[Memory.State.Characters[Character].Abilities[i]].Description;
                                }
                                else
                                {
                                    ITEM[i, 1] = null;
                                    //Descriptions[i] = "";
                                }
                                BLANKS[i] = false;
                            }
                            else
                            {
                                ITEM[i, 0] = null;
                                ITEM[i, 1] = null;
                                BLANKS[i] = true;
                                //Descriptions[i] = "";
                            }
                        }
                    }
                }
            }
        }
    
}