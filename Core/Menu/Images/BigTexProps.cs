namespace OpenVIII
{
    public abstract partial class SP2
    {
        #region Classes

        /// <summary>
        /// For big textures.
        /// </summary>
        public class BigTexProps : TexProps
        {
            /// <summary>
            /// leave null unless big version has a different custom palette than normal.
            /// </summary>
            //public Color[] Colors;

            /// <summary>
            /// Filename; To match more than one number use {0:00} or {00:00} for ones with leading zeros.
            /// </summary>
            //public string Filename;

            #region Fields

            /// <summary>
            /// Big versions of textures take the file and split it into multiple. How many splits
            /// per BigFilename. Value to be interval of 2. As these files are all 2 cols wide. And
            /// must be &gt;= 2
            /// if split is &lt;= 1 then the file is not split up into parts. They are just larger versions.
            /// </summary>
            public uint Split = 1;

            #endregion Fields

            //public BigTexProps(string filename, uint split, Color[] colors = null)
            //{
            //    Filename = filename;
            //    Split = split;
            //    Colors = colors;
            //}
        }

        #endregion Classes
    }
}