#pragma warning disable CS0649

namespace OpenVIII
{
    public static partial class Memory
    {
        public static class Archives
        {
            public static Archive ZZZ_MAIN { get; private set; }
            public static Archive ZZZ_OTHER { get; private set; }
            public static Archive A_BATTLE { get; private set; }
            public static Archive A_FIELD { get; private set; }
            public static Archive A_MAGIC { get; private set; }
            public static Archive A_MAIN { get; private set; }
            public static Archive A_MENU { get; private set; }
            public static Archive A_WORLD { get; private set; }
            public static Archive A_MOVIES { get; private set; }

            public static void init()
            {
                Archive parent = new Archive(FF8DIR);
                ZZZ_MAIN = new Archive("main.zzz", true, parent);
                ZZZ_OTHER = new Archive("other.zzz", true,  parent);
                A_BATTLE = new Archive("battle", FF8DIRdata_lang, ZZZ_MAIN);
                A_FIELD = new Archive("field", FF8DIRdata_lang, ZZZ_MAIN);
                A_MAGIC = new Archive("magic", FF8DIRdata_lang, ZZZ_MAIN);
                A_MAIN = new Archive("main", FF8DIRdata_lang, ZZZ_MAIN);
                A_MENU = new Archive("menu", FF8DIRdata_lang, ZZZ_MAIN);
                A_WORLD = new Archive("world", FF8DIRdata_lang, ZZZ_MAIN);
                A_MOVIES = new Archive("movies", FF8DIRdata_lang, ZZZ_OTHER, FF8DIRdata);
            }

            public static Archive ConvertPath(string path) => new Archive(path);
        }
    }
}