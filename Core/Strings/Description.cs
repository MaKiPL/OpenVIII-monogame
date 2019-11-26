namespace OpenVIII
{
    public partial class Strings
    {
        #region Classes

        public static class Description
        {
            #region Properties

            public static FF8StringReference Ability => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 224);
            public static FF8StringReference Auto => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 222);
            public static FF8StringReference AutoAtk => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 270);
            public static FF8StringReference AutoDef => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 274);
            public static FF8StringReference AutoMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 272);
            public static FF8StringReference EL_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 248);
            public static FF8StringReference EL_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 254);
            public static FF8StringReference EL_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 250);
            public static FF8StringReference GF => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 263);
            public static FF8StringReference Hit => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 240);
            public static FF8StringReference HP => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 226);
            public static FF8StringReference Junction => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 218);
            public static FF8StringReference Luck => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 238);
            public static FF8StringReference Mag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 232);
            public static FF8StringReference Magic => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 265);
            public static FF8StringReference Off => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 220);
            public static FF8StringReference RemAll => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 276);
            public static FF8StringReference RemMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 278);
            public static FF8StringReference Spd => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 236);
            public static FF8StringReference Spr => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 234);
            public static FF8StringReference ST_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 244);
            public static FF8StringReference ST_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 252);
            public static FF8StringReference ST_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 246);
            public static FF8StringReference Stats => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 256);
            public static FF8StringReference Vit => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 230);
            public static FF8StringReference Str => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 228);

            #endregion Properties
            public static class SideMenu
            {
                public static FF8StringReference Junction => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 1);
                public static FF8StringReference Item => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 3);
                public static FF8StringReference Magic => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 5);
                public static FF8StringReference GF => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 7);
                public static FF8StringReference Status => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 9);
                public static FF8StringReference Card => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 11);
                public static FF8StringReference Rearrange => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 13);
                public static FF8StringReference Save => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 15);
                public static FF8StringReference Config => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 17);
                public static FF8StringReference PastParty => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 19);
                public static FF8StringReference Ability => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 63);
                public static FF8StringReference Switch => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 65);
                public static FF8StringReference SLV => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 70);
                public static FF8StringReference Information => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 76);
                public static FF8StringReference Tutorial => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 68);
                public static FF8StringReference TEST => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 78);
                public static FF8StringReference Review => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 80);
                /// <summary>
                /// Ace
                /// </summary>
                public static FF8StringReference A => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 72);
                public static FF8String Battle { get; } = "Test Battle Menu";
            }
        }

        #endregion Classes
    }
}