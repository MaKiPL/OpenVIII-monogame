namespace FF8
{






    internal static partial class Saves
    {
        internal enum Characters
        {
            // I noticed some values were in order of these characters so I made those values into arrays
            // and put the character names into an enum.
            Squall_Leonhart = 0,
            Zell_Dincht,
            Irvine_Kinneas,
            Quistis_Trepe,
            Rinoa_Heartilly,
            Selphie_Tilmitt,
            Seifer_Almasy,
            Edea_Kramer,
            Laguna_Loire = Squall_Leonhart, //Laguna always replaces squall
            Kiros_Seagill, //unsure who kiros and ward replace. I think it's whom ever is with squall.
            Ward_Zabac,
            Blank = 0xFF
        }
    }
}