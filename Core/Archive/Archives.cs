#pragma warning disable CS0649

namespace OpenVIII
{

    public static partial class Memory
    {
        public static class Archives 
        {
            public static Archive A_BATTLE = new Archive(FF8DIRdata_lang, "battle");
            public static Archive A_FIELD = new Archive(FF8DIRdata_lang, "field");
            public static Archive A_MAGIC = new Archive(FF8DIRdata_lang, "magic");
            public static Archive A_MAIN = new Archive(FF8DIRdata_lang, "main");
            public static Archive A_MENU = new Archive(FF8DIRdata_lang, "menu");
            public static Archive A_WORLD = new Archive(FF8DIRdata_lang, "world");
            public static Archive A_OTHER = new Archive(FF8DIRdata_lang, "other");
            public static Archive ConvertPath(string path) => new Archive(path, "");
        }
    }
}