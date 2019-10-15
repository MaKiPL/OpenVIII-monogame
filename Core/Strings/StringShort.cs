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
        }

        public static class Name
        {
            #region Properties

            public static FF8StringReference _ => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 266);
            public static FF8StringReference Ability => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 223);
            public static FF8StringReference Auto => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 221);
            public static FF8StringReference AutoAtk => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 269);
            public static FF8StringReference AutoDef => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 273);
            public static FF8StringReference AutoMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 271);
            public static FF8StringReference CurrentEXP => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 23);
            public static FF8StringReference EL_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 247);
            public static FF8StringReference EL_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 253);
            public static FF8StringReference EL_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 249);
            public static FF8StringReference ForwardSlash => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 25);
            public static FF8StringReference GF => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 262);
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
            public static FF8StringReference Luck => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 237);
            public static FF8StringReference LV => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 27);
            public static FF8StringReference Mag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 231);
            public static FF8StringReference Magic => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 264);
            public static FF8StringReference NextLEVEL => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 24);
            public static FF8StringReference Off => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 219);
            public static FF8StringReference Percent => Memory.Strings.Read(Strings.FileID.MNGRP, 0, 29);
            public static FF8StringReference RemAll => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 275);
            public static FF8StringReference RemMag => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 277);
            public static FF8StringReference Spd => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 235);
            public static FF8StringReference Spr => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 233);
            public static FF8StringReference ST_A => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 243);
            public static FF8StringReference ST_A_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 251);
            public static FF8StringReference ST_A2 => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 257);
            public static FF8StringReference ST_D => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 245);
            public static FF8StringReference Stats => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 255);
            public static FF8StringReference Str => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 227);
            public static FF8StringReference Vit => Memory.Strings.Read(Strings.FileID.MNGRP, 2, 229);

            #endregion Properties
        }

        #endregion Classes
    }
}