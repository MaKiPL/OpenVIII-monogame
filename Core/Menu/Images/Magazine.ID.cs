using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVIII
{
    partial class Magazine
    {
        #region Enums

        /// <summary>
        /// ID's of Magazine Images.
        /// </summary>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Weapons_Monthly"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Pet_Pals"/>
        /// <seealso cref="https://finalfantasy.fandom.com/wiki/Occult_Fan"/>
        public new enum ID
        {
            //Weapons Monthly, 1st Issue
            Lion_Heart,
            Shooting_Star,
            Exeter,
            Strange_Vision,
            //Weapons Monthly, March Issue
            Revolver,
            Metal_Knuckle,
            Flail,
            Chain_Whip,
            //Weapons Monthly, April Issue
            Shear_Trigger,
            Maverick,
            Pinwheel,
            Valiant,
            //Weapons Monthly, May Issue
            Cutting_Trigger,
            Valkyrie,
            Ulysses,
            Slaying_Tail,
            //Weapons Monthly, June Issue 
            Flame_Saber,
            Gauntlet,
            Morning_Star,
            Red_Scorpion,
            //Weapons Monthly, July Issue
            Twin_Lance,
            Rising_Sun,
            Bismarck,
            Crescent_Wish,
            //Weapons Monthly, August Issue
            Punishment,
            Ehrgeiz,
            Cardinal,
            Save_the_Queen,
            //Pet Pals
            Pet_Pals_Vol_1,
            Pet_Pals_Vol_2,
            Pet_Pals_Vol_3,
            Pet_Pals_Vol_4,
            Pet_Pals_Vol_5,
            Pet_Pals_Vol_6,
            //Occult Fan
            Occult_Fan_I_1,
            Occult_Fan_I_2,
            Occult_Fan_II_1,
            Occult_Fan_II_2,
            Occult_Fan_III,
            Occult_Fan_IV_1,
            Occult_Fan_IV_2,
            //Tiple Triad Tutorial
            Triple_Triad_Tutorial_1,
            Triple_Triad_Tutorial_2,
            Triple_Triad_Tutorial_3,
            Triple_Triad_Tutorial_4,
            Triple_Triad_Tutorial_5,
            Triple_Triad_Tutorial_7,
            Triple_Triad_Tutorial_8,
            Triple_Triad_Tutorial_9,
            Triple_Triad_Tutorial_10,
            //Tutorial
            Tutorial_1,
            Tutorial_2,
            Tutorial_3,
            Tutorial_4,
            Tutorial_5,
            Tutorial_6,
            Tutorial_7,
            Tutorial_8,
            Tutorial_9,
            Tutorial_10,
            //Cute Comic
            Cute_Comic_1,
            Cute_Comic_2,
            Cute_Comic_3,
            //Chocobo World Tutorial
            Chocobo_World_Tutorial_1,
            Chocobo_World_Tutorial_2,
            Chocobo_World_Tutorial_3,
            Chocobo_World_Tutorial_4,
            Chocobo_World_Tutorial_5,
            Chocobo_World_Tutorial_6,
            Chocobo_World_Tutorial_7,
            Chocobo_World_Tutorial_8,
            Chocobo_World_Tutorial_9,
            Chocobo_World_Tutorial_10,
            Chocobo_World_Tutorial_11,
            Chocobo_World_Tutorial_12,
            Chocobo_World_Tutorial_13,
            Chocobo_World_Tutorial_14,
            Chocobo_World_Tutorial_15,
            Chocobo_World_Tutorial_16,
            Chocobo_World_Tutorial_17,
            Chocobo_World_Tutorial_18,
            Chocobo_World_Tutorial_19,
            Chocobo_World_Tutorial_20,
            Chocobo_World_Tutorial_21,
            Chocobo_World_Tutorial_22,
            Chocobo_World_Tutorial_23,
            Chocobo_World_Tutorial_24,
            Chocobo_World_Tutorial_25,
            Chocobo_World_Tutorial_26_1,
            Chocobo_World_Tutorial_26_2,
            //Icon like items.
            Blue_1,
            Blue_2,
            Blue_3,
            Blue_4,
            Blue_5,
            Blue_6,
            Blue_7,
            Red_Arrow_Right,
            Red_Arrow_Down,
            Red_Pipe,
            Red_Forward_Slash,
            Red_Back_Slash,
            Red_Dash,
            //Everything in mag17.tex is in Icons so skipping it.
            //Sketch versions of Cute Comic
            Cute_Comic_4,
            Cute_Comic_5,
            Cute_Comic_6,
            //mag18.tex and mag19.tex are the same thing
            //magita
            Mag_BG
        }

        #endregion Enums
    }
}
