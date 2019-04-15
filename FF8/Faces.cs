namespace FF8
{

    internal class Faces : SP2
    {
        #region Fields

        //protected static Texture2D[] textures;

        //protected static Dictionary<Enum, Entry> entries;

        #endregion Fields

        #region Constructors

        public Faces()
        {
            TextureFilename = "face{0:0}.tex";
            TextureStartOffset = 1;
            IndexFilename = "face.sp2";
            TextureCount = 2;
            EntriesPerTexture = 16;
            Init();
        }

        #endregion Constructors

        #region Enums

        /// <summary>
        /// First half in faces1.tex, second half in faces2.tex, 8 cols 2 rows per file.
        /// </summary>
        public new enum ID
        {
            Squall_Leonhart = 0,
            Zell_Dincht,
            Irvine_Kinneas,
            Quistis_Trepe,
            Rinoa_Heartilly,
            Selphie_Tilmitt,
            Seifer_Almasy,
            Edea_Kramer,
            Laguna_Loire,
            Kiros_Seagill,
            Ward_Zabac,
            Lion = Ward_Zabac + 2, //skipped blank
            MiniMog,
            Boko,
            Angelo,
            Quezacotl,
            Shiva,
            Ifrit,
            Siren,
            Brothers,
            Diablos,
            Carbuncle,
            Leviathan,
            Pandemona,
            Cerberus,
            Alexander,
            Doomtrain,
            Bahamut,
            Cactuar,
            Tonberry,
            Eden
        }

        #endregion Enums
    }
}