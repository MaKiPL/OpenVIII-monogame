using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenVIII
{
    public partial class IGM_Junction
    {
        #region Classes

        private abstract class IGMData_Values : IGMData
        {
            #region Fields

            public static EventHandler<Font.ColorID> ColorChangeEventListener;
            protected Saves.CharacterData prevSetting;
            private bool eventAdded = false;

            #endregion Fields

            #region Constructors

            public IGMData_Values(int count, int depth, IGMDataItem container = null, int? cols = null, int? rows = null) : base(count, depth, container, cols, rows)
            {
            }

            #endregion Constructors

            #region Methods

            public override void Refresh()
            {
                //prevSetting = prevSetting;
                if (!eventAdded)
                {
                    IGMData_Mag_ST_A_D_Slots.PrevSettingUpdateEventListener += PrevSettingEvent;
                    eventAdded = true;
                }
                base.Refresh();
            }

            protected void FillData<T>(Dictionary<T, byte> oldtotal, Dictionary<T, byte> total, Enum[] availableFlagsarray, Icons.ID starticon, sbyte offset = 0, byte palette = 2, Icons.ID[] skip = null) where T : Enum
            {
                int _nag = 0;
                int _pos = 0;
                sbyte endoffset = (sbyte)(offset > 0 ? offset : 0);

                for (sbyte pos = 0; pos < Count - endoffset; pos++)
                {
                    if (skip != null && skip.Contains(starticon + pos))
                    {
                        offset -= 1;
                        pos += 1;
                        endoffset -= 1;
                    }
                    ITEM[pos + offset, 0] = new IGMDataItem_Icon(starticon + pos, new Rectangle(SIZE[pos + offset].X, SIZE[pos + offset].Y, 0, 0), palette);
                    ITEM[pos + offset, 1] = total[(T)availableFlagsarray[pos + 1]] > 100 ? new IGMDataItem_Icon(Icons.ID.Star, new Rectangle(SIZE[pos + offset].X + 45, SIZE[pos + offset].Y, 0, 0), 4) : null;
                    ITEM[pos + offset, 2] = null;
                    ITEM[pos + offset, 3] = new IGMDataItem_Int(
                            total[(T)availableFlagsarray[pos + 1]] > 100 ?
                            total[(T)availableFlagsarray[pos + 1]] - 100 :
                            total[(T)availableFlagsarray[pos + 1]], new Rectangle(SIZE[pos + offset].X + SIZE[pos + offset].Width - 80, SIZE[pos + offset].Y, 0, 0), 17, numtype: Icons.NumType.sysFntBig, spaces: 3);
                    ITEM[pos + offset, 4] = new IGMDataItem_String("%", new Rectangle(SIZE[pos + offset].X + SIZE[pos + offset].Width - 20, SIZE[pos + offset].Y, 0, 0));
                    if (oldtotal != null)
                    {
                        if (oldtotal[(T)availableFlagsarray[pos + 1]] > total[(T)availableFlagsarray[pos + 1]])
                        {
                            ((IGMDataItem_Icon)ITEM[pos + offset, 0]).Palette = 5;
                            ((IGMDataItem_Icon)ITEM[pos + offset, 0]).Faded_Palette = 5;
                            ITEM[pos + offset, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Down, new Rectangle(SIZE[pos + offset].X + SIZE[pos + offset].Width - 105, SIZE[pos + offset].Y, 0, 0), 16);
                            ((IGMDataItem_Int)ITEM[pos + offset, 3]).FontColor = Font.ColorID.Red;
                            ((IGMDataItem_String)ITEM[pos + offset, 4]).FontColor = Font.ColorID.Red;

                            //{
                            //    ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Palette = 5;
                            //    ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Faded_Palette = 5;
                            //    ((IGMDataItem_String)Slots.ITEM[Slots.CURSOR_SELECT, 1]).Colorid = Font.ColorID.Red;
                            //}
                            if (++_nag > _pos)
                                ColorChangeEventListener?.Invoke(this, Font.ColorID.Red);
                        }
                        else if (oldtotal[(T)availableFlagsarray[pos + 1]] < total[(T)availableFlagsarray[pos + 1]])
                        {
                            ((IGMDataItem_Icon)ITEM[pos + offset, 0]).Palette = 6;
                            ((IGMDataItem_Icon)ITEM[pos + offset, 0]).Faded_Palette = 6;
                            ITEM[pos + offset, 2] = new IGMDataItem_Icon(Icons.ID.Arrow_Up, new Rectangle(SIZE[pos + offset].X + SIZE[pos + offset].Width - 105, SIZE[pos + offset].Y, 0, 0), 17);
                            ((IGMDataItem_Int)ITEM[pos + offset, 3]).FontColor = Font.ColorID.Yellow;
                            ((IGMDataItem_String)ITEM[pos + offset, 4]).FontColor = Font.ColorID.Yellow;

                            //{
                            //    ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Palette = 6;
                            //    ((IGMDataItem_Icon)Slots.ITEM[Slots.CURSOR_SELECT, 0]).Faded_Palette = 6;
                            //    ((IGMDataItem_String)Slots.ITEM[Slots.CURSOR_SELECT, 1]).Colorid = Font.ColorID.Yellow;
                            //}
                            if (_nag <= ++_pos)
                                ColorChangeEventListener?.Invoke(this, Font.ColorID.Yellow);
                        }
                    }
                }
            }

            protected Dictionary<T, byte> getTotal<T>(out Enum[] availableFlagsarray, byte max, Kernel_bin.Stat stat, params byte[] spell) where T : Enum
            {
                Dictionary<T, byte> total = new Dictionary<T, byte>(8);

                IEnumerable<Enum> availableFlags = Enum.GetValues(typeof(T)).Cast<Enum>();
                foreach (Enum flag in availableFlags.Where(d => !total.ContainsKey((T)d)))
                    total.Add((T)flag, 0);
                for (int i = 0; i < spell.Length; i++)
                {
                    Enum flags = null;
                    switch (stat)
                    {
                        case Kernel_bin.Stat.EL_Atk:
                            flags = Kernel_bin.MagicData[spell[i]].EL_Atk;
                            break;

                        case Kernel_bin.Stat.EL_Def_1:
                        case Kernel_bin.Stat.EL_Def_2:
                        case Kernel_bin.Stat.EL_Def_3:
                        case Kernel_bin.Stat.EL_Def_4:
                            flags = Kernel_bin.MagicData[spell[i]].EL_Def;
                            break;

                        case Kernel_bin.Stat.ST_Atk:
                            flags = Kernel_bin.MagicData[spell[i]].ST_Atk;
                            break;

                        case Kernel_bin.Stat.ST_Def_1:
                        case Kernel_bin.Stat.ST_Def_2:
                        case Kernel_bin.Stat.ST_Def_3:
                        case Kernel_bin.Stat.ST_Def_4:
                            flags = Kernel_bin.MagicData[spell[i]].ST_Def;
                            break;
                    }
                    if (flags != null && Damageable.GetCharacterData(out Saves.CharacterData c))
                        foreach (Enum flag in availableFlags.Where(flags.HasFlag))
                        {
                            int t = total[(T)flag] + ((Kernel_bin.MagicData[spell[i]].J_Val[stat] * c.Magics[spell[i]]) / 100);
                            total[(T)flag] = (byte)MathHelper.Clamp(t,0,max);
                        }
                    else
                        throw new Exception($"Unknown stat, {stat}");
                }
                availableFlagsarray = availableFlags.ToArray();
                return total;
            }

            protected void PrevSettingEvent(object sender, Saves.CharacterData e) =>
                                //shadowcopy
                                prevSetting = e;

            #endregion Methods
        }

        #endregion Classes
    }
}