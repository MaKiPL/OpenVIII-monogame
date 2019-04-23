using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace FF8
{
    internal partial class Magazine
    {
        #region Fields

        private new Dictionary<ID, Entry> Entries;

        #endregion Fields

        #region Methods

        protected override void InsertCustomEntries()
        {
            Entries = new Dictionary<ID, Entry>
            {
                //Weapons Monthly, 1st Issue
                [ID.Lion_Heart] = new Entry
                {
                    File = 0,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Shooting_Star] = new Entry
                {
                    File = 0,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Exeter] = new Entry
                {
                    File = 0,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Strange_Vision] = new Entry
                {
                    File = 0,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, March Issue
                [ID.Revolver] = new Entry
                {
                    File = 1,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Metal_Knuckle] = new Entry
                {
                    File = 1,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Flail] = new Entry
                {
                    File = 1,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Chain_Whip] = new Entry
                {
                    File = 1,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, April Issue
                [ID.Shear_Trigger] = new Entry
                {
                    File = 2,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Maverick] = new Entry
                {
                    File = 2,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Pinwheel] = new Entry
                {
                    File = 2,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Valiant] = new Entry
                {
                    File = 2,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, May Issue
                [ID.Cutting_Trigger] = new Entry
                {
                    File = 3,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Valkyrie] = new Entry
                {
                    File = 3,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Ulysses] = new Entry
                {
                    File = 3,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Slaying_Tail] = new Entry
                {
                    File = 3,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, June Issue
                [ID.Flame_Saber] = new Entry
                {
                    File = 4,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Gauntlet] = new Entry
                {
                    File = 4,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Morning_Star] = new Entry
                {
                    File = 4,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Red_Scorpion] = new Entry
                {
                    File = 4,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, July Issue
                [ID.Twin_Lance] = new Entry
                {
                    File = 5,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Rising_Sun] = new Entry
                {
                    File = 5,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Bismarck] = new Entry
                {
                    File = 5,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Crescent_Wish] = new Entry
                {
                    File = 5,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Weapons Monthly, August Issue
                [ID.Punishment] = new Entry
                {
                    File = 6,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Ehrgeiz] = new Entry
                {
                    File = 6,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Cardinal] = new Entry
                {
                    File = 6,
                    X = 0,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Save_the_Queen] = new Entry
                {
                    File = 6,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Pet Pals
                [ID.Pet_Pals_Vol_1] = new Entry
                {
                    File = 7,
                    X = 0,
                    Y = 0,
                    Width = 56,
                    Height = 72,
                },
                [ID.Pet_Pals_Vol_2] = new Entry
                {
                    File = 7,
                    X = 56,
                    Y = 0,
                    Width = 56,
                    Height = 72,
                },
                [ID.Pet_Pals_Vol_3] = new Entry
                {
                    File = 7,
                    X = 112,
                    Y = 0,
                    Width = 56,
                    Height = 72,
                },
                [ID.Pet_Pals_Vol_4] = new Entry
                {
                    File = 7,
                    X = 156,
                    Y = 0,
                    Width = 56,
                    Height = 72,
                },
                [ID.Pet_Pals_Vol_5] = new Entry
                {
                    File = 7,
                    X = 0,
                    Y = 72,
                    Width = 56,
                    Height = 72,
                },
                [ID.Pet_Pals_Vol_6] = new Entry
                {
                    File = 7,
                    X = 56,
                    Y = 72,
                    Width = 56,
                    Height = 72,
                },
                //Occult Fan
                [ID.Occult_Fan_I_1] = new Entry
                {
                    File = 8,
                    X = 0,
                    Y = 0,
                    Width = 112,
                    Height = 118,
                },
                [ID.Occult_Fan_I_2] = new Entry
                {
                    File = 8,
                    X = 0,
                    Y = 121,
                    Width = 55,
                    Height = 66,
                },
                [ID.Occult_Fan_II_1] = new Entry
                {
                    File = 8,
                    X = 169,
                    Y = 0,
                    Width = 87,
                    Height = 92,
                },
                [ID.Occult_Fan_II_2] = new Entry
                {
                    File = 8,
                    X = 160,
                    Y = 96,
                    Width = 96,
                    Height = 92,
                },
                [ID.Occult_Fan_III] = new Entry
                {
                    File = 9,
                    X = 96,
                    Y = 0,
                    Width = 110,
                    Height = 92,
                },
                [ID.Occult_Fan_IV_1] = new Entry
                {
                    File = 9,
                    X = 0,
                    Y = 0,
                    Width = 96,
                    Height = 104,
                },
                [ID.Occult_Fan_IV_2] = new Entry
                {
                    File = 9,
                    X = 0,
                    Y = 104,
                    Width = 88,
                    Height = 88,
                },
                //Tiple Triad Tutorial
                [ID.Triple_Triad_Tutorial_1] = new Entry
                {
                    File = 10,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_2] = new Entry
                {
                    File = 10,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_3] = new Entry
                {
                    File = 11,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_4] = new Entry
                {
                    File = 11,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_5] = new Entry
                {
                    File = 11,
                    X = 96,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_10] = new Entry
                {
                    File = 11,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_7] = new Entry
                {
                    File = 12,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_8] = new Entry
                {
                    File = 12,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_9] = new Entry
                {
                    File = 12,
                    X = 96,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Triple_Triad_Tutorial_10] = new Entry
                {
                    File = 12,
                    X = 128,
                    Y = 96,
                    Width = 128,
                    Height = 96,
                },
                //Tutorial
                [ID.Tutorial_1] = new Entry
                {
                    File = 13,
                    X = 0,
                    Y = 0,
                    Width = 150,
                    Height = 48,
                },
                [ID.Tutorial_2] = new Entry
                {
                    File = 13,
                    X = 0,
                    Y = 64,
                    Width = 68,
                    Height = 64,
                },
                [ID.Tutorial_3] = new Entry
                {
                    File = 13,
                    X = 154,
                    Y = 64,
                    Width = 154,
                    Height = 64,
                },
                [ID.Tutorial_4] = new Entry
                {
                    File = 13,
                    X = 0,
                    Y = 128,
                    Width = 128,
                    Height = 64,
                },
                [ID.Tutorial_5] = new Entry
                {
                    File = 14,
                    X = 0,
                    Y = 0,
                    Width = 98,
                    Height = 64,
                },
                [ID.Tutorial_6] = new Entry
                {
                    File = 14,
                    X = 152,
                    Y = 33,
                    Width = 68,
                    Height = 31,
                },
                [ID.Tutorial_7] = new Entry
                {
                    File = 14,
                    X = 0,
                    Y = 64,
                    Width = 68,
                    Height = 64,
                },
                [ID.Tutorial_8] = new Entry
                {
                    File = 14,
                    X = 80,
                    Y = 77,
                    Width = 119,
                    Height = 51,
                },
                [ID.Tutorial_9] = new Entry
                {
                    File = 14,
                    X = 0,
                    Y = 128,
                    Width = 166,
                    Height = 64,
                },
                [ID.Tutorial_10] = new Entry
                {
                    File = 14,
                    X = 192,
                    Y = 176,
                    Width = 24,
                    Height = 16,
                },
                //Cute Comic
                [ID.Cute_Comic_1] = new Entry
                {
                    File = 15,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Cute_Comic_2] = new Entry
                {
                    File = 15,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Cute_Comic_3] = new Entry
                {
                    File = 15,
                    X = 96,
                    Y = 0,
                    Width = 256,
                    Height = 96,
                },
                //Chocobo World Tutorial
                [ID.Chocobo_World_Tutorial_1] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 112,
                },
                [ID.Chocobo_World_Tutorial_2] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 0,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_3] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 0,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_4] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 0,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_5] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 0,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_6] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 32,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_7] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 32,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_8] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 32,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_9] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 32,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_10] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 64,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_11] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 64,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_12] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 64,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_13] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 64,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_14] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 96,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_15] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 96,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_16] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 96,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_17] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 96,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_18] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 128,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_19] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 128,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_20] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 128,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_21] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 128,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_22] = new Entry
                {
                    File = 16,
                    X = 128,
                    Y = 160,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_23] = new Entry
                {
                    File = 16,
                    X = 160,
                    Y = 160,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_24] = new Entry
                {
                    File = 16,
                    X = 192,
                    Y = 160,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_25] = new Entry
                {
                    File = 16,
                    X = 224,
                    Y = 160,
                    Width = 32,
                    Height = 32,
                },
                [ID.Chocobo_World_Tutorial_26_1] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 112,
                    Width = 16,
                    Height = 16,
                    Offset = new Vector2(-16, 16),
                },
                [ID.Chocobo_World_Tutorial_26_2] = new Entry
                {
                    File = 16,
                    X = 16,
                    Y = 112,
                    Width = 96,
                    Height = 72,
                },
                //Icon Like Items
                [ID.Blue_1] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 128,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_2] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 144,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_3] = new Entry
                {
                    File = 16,
                    X = 112,
                    Y = 112,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_4] = new Entry
                {
                    File = 16,
                    X = 112,
                    Y = 128,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_5] = new Entry
                {
                    File = 16,
                    X = 112,
                    Y = 144,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_6] = new Entry
                {
                    File = 16,
                    X = 112,
                    Y = 160,
                    Width = 16,
                    Height = 16,
                },
                [ID.Blue_7] = new Entry
                {
                    File = 16,
                    X = 112,
                    Y = 176,
                    Width = 16,
                    Height = 16,
                },
                [ID.Red_Arrow_Right] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 160,
                    Width = 16,
                    Height = 16,
                },
                [ID.Red_Arrow_Down] = new Entry
                {
                    File = 16,
                    X = 0,
                    Y = 176,
                    Width = 16,
                    Height = 16,
                },
                [ID.Red_Pipe] = new Entry
                {
                    File = 16,
                    X = 16,
                    Y = 184,
                    Width = 8,
                    Height = 8,
                },
                [ID.Red_Forward_Slash] = new Entry
                {
                    File = 16,
                    X = 24,
                    Y = 184,
                    Width = 8,
                    Height = 8,
                },
                [ID.Red_Back_Slash] = new Entry
                {
                    File = 16,
                    X = 32,
                    Y = 184,
                    Width = 8,
                    Height = 8,
                },
                [ID.Red_Dash] = new Entry
                {
                    File = 16,
                    X = 40,
                    Y = 184,
                    Width = 8,
                    Height = 8,
                },
                //Sketch versions of Cute Comic
                [ID.Cute_Comic_4] = new Entry
                {
                    File = 18,
                    X = 0,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Cute_Comic_5] = new Entry
                {
                    File = 18,
                    X = 128,
                    Y = 0,
                    Width = 128,
                    Height = 96,
                },
                [ID.Cute_Comic_6] = new Entry
                {
                    File = 18,
                    X = 96,
                    Y = 0,
                    Width = 256,
                    Height = 96,
                },
                //magita.tex
                [ID.Mag_BG] = new Entry
                {
                    File = 20,
                    X = 0,
                    Y = 0,
                    Width = 32,
                    Height = 32,
                },
            };
        }

        #endregion Methods
    }
}