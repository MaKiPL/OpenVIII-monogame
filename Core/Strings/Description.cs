using System.Diagnostics.CodeAnalysis;

namespace OpenVIII
{
    public partial class Strings
    {
        #region Classes

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public static class Description
        {
            #region Properties

            public static FF8StringReference Ability => Memory.Strings.Read(FileID.MenuGroup, 2, 224);
            public static FF8StringReference Auto => Memory.Strings.Read(FileID.MenuGroup, 2, 222);
            public static FF8StringReference AutoAtk => Memory.Strings.Read(FileID.MenuGroup, 2, 270);
            public static FF8StringReference AutoDef => Memory.Strings.Read(FileID.MenuGroup, 2, 274);
            public static FF8StringReference AutoMag => Memory.Strings.Read(FileID.MenuGroup, 2, 272);
            public static FF8StringReference ElA => Memory.Strings.Read(FileID.MenuGroup, 2, 248);
            public static FF8StringReference ElAd => Memory.Strings.Read(FileID.MenuGroup, 2, 254);
            public static FF8StringReference ElD => Memory.Strings.Read(FileID.MenuGroup, 2, 250);
            public static FF8StringReference GF => Memory.Strings.Read(FileID.MenuGroup, 2, 263);
            public static FF8StringReference Hit => Memory.Strings.Read(FileID.MenuGroup, 2, 240);
            public static FF8StringReference HP => Memory.Strings.Read(FileID.MenuGroup, 2, 226);
            public static FF8StringReference Junction => Memory.Strings.Read(FileID.MenuGroup, 2, 218);
            public static FF8StringReference Luck => Memory.Strings.Read(FileID.MenuGroup, 2, 238);
            public static FF8StringReference Mag => Memory.Strings.Read(FileID.MenuGroup, 2, 232);
            public static FF8StringReference Magic => Memory.Strings.Read(FileID.MenuGroup, 2, 265);
            public static FF8StringReference Off => Memory.Strings.Read(FileID.MenuGroup, 2, 220);
            public static FF8StringReference RemAll => Memory.Strings.Read(FileID.MenuGroup, 2, 276);
            public static FF8StringReference RemMag => Memory.Strings.Read(FileID.MenuGroup, 2, 278);
            public static FF8StringReference Spd => Memory.Strings.Read(FileID.MenuGroup, 2, 236);
            public static FF8StringReference Spr => Memory.Strings.Read(FileID.MenuGroup, 2, 234);
            public static FF8StringReference StA => Memory.Strings.Read(FileID.MenuGroup, 2, 244);
            public static FF8StringReference StAd => Memory.Strings.Read(FileID.MenuGroup, 2, 252);
            public static FF8StringReference StD => Memory.Strings.Read(FileID.MenuGroup, 2, 246);
            public static FF8StringReference Stats => Memory.Strings.Read(FileID.MenuGroup, 2, 256);
            public static FF8StringReference Vit => Memory.Strings.Read(FileID.MenuGroup, 2, 230);
            public static FF8StringReference Str => Memory.Strings.Read(FileID.MenuGroup, 2, 228);

            public static FF8StringReference Use => Memory.Strings.Read(FileID.MenuGroup, 2, 180);

            public static FF8StringReference Rearrange => Memory.Strings.Read(FileID.MenuGroup, 2, 184);

            public static FF8StringReference Sort => Memory.Strings.Read(FileID.MenuGroup, 2, 203);

            public static FF8StringReference Battle => Memory.Strings.Read(FileID.MenuGroup, 2, 182);

            #endregion Properties
            [SuppressMessage("ReSharper", "MemberHidesStaticFromOuterClass")]
            public static class SideMenu
            {
                public static FF8StringReference Junction => Memory.Strings.Read(FileID.MenuGroup, 0, 1);
                public static FF8StringReference Item => Memory.Strings.Read(FileID.MenuGroup, 0, 3);
                public static FF8StringReference Magic => Memory.Strings.Read(FileID.MenuGroup, 0, 5);
                public static FF8StringReference GF => Memory.Strings.Read(FileID.MenuGroup, 0, 7);
                public static FF8StringReference Status => Memory.Strings.Read(FileID.MenuGroup, 0, 9);
                public static FF8StringReference Card => Memory.Strings.Read(FileID.MenuGroup, 0, 11);
                public static FF8StringReference Rearrange => Memory.Strings.Read(FileID.MenuGroup, 0, 13);
                public static FF8StringReference Save => Memory.Strings.Read(FileID.MenuGroup, 0, 15);
                public static FF8StringReference Config => Memory.Strings.Read(FileID.MenuGroup, 0, 17);
                public static FF8StringReference PastParty => Memory.Strings.Read(FileID.MenuGroup, 0, 19);
                public static FF8StringReference Ability => Memory.Strings.Read(FileID.MenuGroup, 0, 63);
                public static FF8StringReference Switch => Memory.Strings.Read(FileID.MenuGroup, 0, 65);
                public static FF8StringReference SLV => Memory.Strings.Read(FileID.MenuGroup, 0, 70);
                public static FF8StringReference Information => Memory.Strings.Read(FileID.MenuGroup, 0, 76);
                public static FF8StringReference Tutorial => Memory.Strings.Read(FileID.MenuGroup, 0, 68);
                public static FF8StringReference Test => Memory.Strings.Read(FileID.MenuGroup, 0, 78);
                public static FF8StringReference Review => Memory.Strings.Read(FileID.MenuGroup, 0, 80);
                /// <summary>
                /// Ace
                /// </summary>
                public static FF8StringReference A => Memory.Strings.Read(FileID.MenuGroup, 0, 72);
                public static FF8String Battle { get; } = "Test Battle Menu";
            }
        }

        #endregion Classes
    }
}