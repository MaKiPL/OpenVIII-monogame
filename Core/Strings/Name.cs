namespace OpenVIII
{
    public partial class Strings
    {
        #region Classes

        public static class Name
        {
            #region Properties
            public static FF8StringReference New_Game => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 105);
            public static FF8StringReference Load_Game => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 106);
            public static FF8StringReference _ => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 266);
            public static FF8StringReference Ability => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 223);
            public static FF8StringReference Auto => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 221);
            public static FF8StringReference AutoAtk => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 269);
            public static FF8StringReference AutoDef => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 273);
            public static FF8StringReference AutoMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 271);
            public static FF8StringReference BlockToLoad => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 114);
            public static FF8StringReference BlockToSave => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 89);
            public static FF8StringReference Changes_current_HP_OK => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 73);
            public static FF8StringReference CheckGameFolder => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 110);
            public static FF8StringReference CurrentEXP => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 23);
            public static FF8StringReference EL_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 247);
            public static FF8StringReference EL_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 253);
            public static FF8StringReference EL_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 249);
            public static FF8StringReference FF8 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 127);
            public static FF8StringReference ForwardSlash => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 25);
            public static FF8StringReference GameFolder => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 86);
            public static FF8StringReference GameFolderSlot1 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 121);
            public static FF8StringReference GameFolderSlot2 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 122);
            public static FF8StringReference GF => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 262);
            public static FF8StringReference Havent_been_to_a_shop => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 66);
            public static FF8StringReference Hit => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 239);

            /// <summary>
            /// HP
            /// </summary>
            /// <remarks>
            /// static FF8StringReference HP =&gt; Memory.Strings.Read(Strings.FileID.MNGRP, 0, 26);
            /// //alt location
            /// </remarks>
            public static FF8StringReference HP => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 225);

            public static FF8StringReference Junction => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 217);
            public static FF8StringReference Load => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 54);
            public static FF8StringReference LoadFF8 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 128);
            public static FF8StringReference Loading => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 93);
            public static FF8StringReference Luck => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 237);
            public static FF8StringReference LV => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 27);
            public static FF8StringReference Mag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 231);
            public static FF8StringReference Magic => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 264);
            public static FF8StringReference NextLEVEL => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 24);
            public static FF8StringReference No => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 58);
            public static FF8StringReference Nothing_happened => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 81);
            public static FF8StringReference Off => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 219);
            public static FF8StringReference Percent => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 29);
            public static FF8StringReference RemAll => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 275);
            public static FF8StringReference RemMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 277);
            public static FF8StringReference Save => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 14);
            public static FF8StringReference SaveFF8 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 117);
            public static FF8StringReference Saving => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 94);
            public static FF8StringReference Slot1 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 87);
            public static FF8StringReference Slot2 => Memory.Strings.Read(Strings.FileID.MNGRP, 1, 88);
            public static FF8StringReference Spd => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 235);
            public static FF8StringReference Spr => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 233);
            public static FF8StringReference ST_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 243);
            public static FF8StringReference ST_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 251);
            public static FF8StringReference ST_A2 => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 257);
            public static FF8StringReference ST_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 245);
            public static FF8StringReference Stats => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 255);
            public static FF8StringReference Str => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 227);
            public static FF8StringReference TIME => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 82);
            public static FF8StringReference TIMER => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 83);
            public static FF8StringReference To_confirm => Memory.Strings.Read(Strings.FileID.KERNEL, 30, 22);
            public static FF8StringReference Vit => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 229);
            public static FF8StringReference Yes => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 57);
            public static FF8String Use => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 179);

            public static FF8String Rearrange => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 183);

            public static FF8String Sort => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 202);

            public static FF8String Battle => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 181);
        #endregion Properties

        #region Classes

        public static class SideMenu
            {
                #region Properties

                public static FF8StringReference A => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 71);
                public static FF8StringReference Ability => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 62);
                public static FF8String Battle { get; } = "Battle";
                public static FF8StringReference Card => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 10);
                public static FF8StringReference Config => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 16);
                public static FF8StringReference GF => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 6);
                public static FF8StringReference Information => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 75);
                public static FF8StringReference Item => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 2);
                public static FF8StringReference Junction => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 0);
                public static FF8StringReference Magic => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 4);

                /// <summary>
                /// ????
                /// </summary>
                public static FF8StringReference PastParty => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 18);

                public static FF8StringReference QMarks2 => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 74);
                public static FF8StringReference Rearrange => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 12);
                public static FF8StringReference Review => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 79);
                public static FF8StringReference Save => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 14);
                public static FF8StringReference SLV => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 69);
                public static FF8StringReference Status => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 8);
                public static FF8StringReference Switch => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 64);
                public static FF8StringReference TEST => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 77);
                public static FF8StringReference Tutorial => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 67);

                #endregion Properties
            }

            #endregion Classes
        }

        #endregion Classes
    }
}