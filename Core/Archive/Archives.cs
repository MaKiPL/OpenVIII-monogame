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
                Memory.Log.WriteLine($"{nameof(Archive)}::{nameof(init)}");
                Archive parent = new Archive(FF8DIR);
                ZZZ_MAIN = new Archive("main.zzz", true, parent);
                ZZZ_OTHER = new Archive("other.zzz", true, parent);
                A_BATTLE = new Archive("battle", FF8DIRdata_lang, ZZZ_MAIN);
                A_FIELD = new Archive("field", FF8DIRdata_lang, ZZZ_MAIN);
                A_MAGIC = new Archive("magic", FF8DIRdata_lang, ZZZ_MAIN);
                A_MAIN = new Archive("main", FF8DIRdata_lang, ZZZ_MAIN);
                A_MENU = new Archive("menu", FF8DIRdata_lang, ZZZ_MAIN);
                A_WORLD = new Archive("world", FF8DIRdata_lang, ZZZ_MAIN);
                A_MOVIES = new Archive("movies", FF8DIRdata_lang, ZZZ_OTHER, FF8DIRdata);

                var aw = ArchiveZZZ.Load(ZZZ_MAIN);
                if (aw != null)
                {
                    void Merge(Archive a)
                    {
                        Memory.Log.WriteLine($"{nameof(Archive)}::{nameof(init)}::{nameof(Merge)}\n\t{a} to {ZZZ_MAIN}");
                        string fs = a.FS;
                        var awchild = ArchiveWorker.Load(a);
                        FI fi = aw.ArchiveMap.FindString(ref fs, out int size);
                        aw.ArchiveMap.MergeMaps(awchild.ArchiveMap, fi.Offset);
                    }
                    Merge(A_BATTLE);
                    Merge(A_FIELD);
                    Merge(A_MAGIC);
                    Merge(A_MAIN);
                    Merge(A_MENU);
                    Merge(A_WORLD);
                    ArchiveBase.PurgeCache();
                    A_BATTLE = ZZZ_MAIN;
                    A_FIELD = ZZZ_MAIN;
                    A_MAGIC = ZZZ_MAIN;
                    A_MAIN = ZZZ_MAIN;
                    A_MENU = ZZZ_MAIN;
                    A_WORLD = ZZZ_MAIN;
                }
                aw = ArchiveZZZ.Load(ZZZ_OTHER);
                
            }

            public static Archive ConvertPath(string path) => new Archive(path);
        }
    }
}