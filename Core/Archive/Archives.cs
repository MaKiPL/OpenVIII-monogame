// ReSharper disable InconsistentNaming
#pragma warning disable CS0649

namespace OpenVIII
{
    public static partial class Memory
    {
        public static bool QuitNextUpdate { get; internal set; } = false;

        public static class Archives
        {
            public static Archive ZZZ_MAIN { get; set; }
            public static Archive ZZZ_OTHER { get; set; }
            public static Archive A_BATTLE { get; set; }
            public static Archive A_FIELD { get; set; }
            public static Archive A_MAGIC { get; set; }
            public static Archive A_MAIN { get; set; }
            public static Archive A_MENU { get; set; }
            public static Archive A_WORLD { get; set; }
            public static Archive A_MOVIES { get; set; }

            public static void Init()
            {
                Memory.Log.WriteLine($"{nameof(Archive)}::{nameof(Init)}");
                var parent = new Archive(FF8Dir);
                ZZZ_MAIN = new Archive("main.zzz", true, parent);
                ZZZ_OTHER = new Archive("other.zzz", true, parent);
                A_BATTLE = new Archive("battle", FF8DirDataLang, ZZZ_MAIN);
                A_FIELD = new Archive("field", FF8DirDataLang, ZZZ_MAIN);
                A_MAGIC = new Archive("magic", FF8DirDataLang, ZZZ_MAIN);
                A_MAIN = new Archive("main", FF8DirDataLang, ZZZ_MAIN);
                A_MENU = new Archive("menu", FF8DirDataLang, ZZZ_MAIN);
                A_WORLD = new Archive("world", FF8DirDataLang, ZZZ_MAIN);
                A_MOVIES = new Archive("movies", FF8DirDataLang, ZZZ_OTHER, FF8DirData);

                /*var aw =*/
                ArchiveZzz.Load(ZZZ_MAIN); //try to load main.zzz also caches it.
                                           //This actually slows things down. I thought it might be a good idea but at least now we know.
                                           //MergeArchiveToZzz(aw); // try to merge all the file maps into the zzz map. Since all the binary data is in there.
                                           /*aw = */
                ArchiveZzz.Load(ZZZ_OTHER); //try to load other.zzz also caches it.
            }

            // ReSharper disable once UnusedMember.Local
            private static void MergeArchiveToZzz(ArchiveBase aw)
            {
                if (aw == null) return;

                void Merge(Archive a)
                {
                    Log.WriteLine($"{nameof(Archive)}::{nameof(Init)}::{nameof(Merge)}\n\t{a} to {ZZZ_MAIN}");
                    var fs = a.FS;
                    var child = ArchiveWorker.Load(a);
                    var fi = aw.ArchiveMap.FindString(ref fs, out var _);
                    aw.ArchiveMap.MergeMaps(child.ArchiveMap, fi.Offset);
                }
                Merge(A_BATTLE);
                Merge(A_FIELD);
                Merge(A_MAGIC);
                Merge(A_MAIN);
                Merge(A_MENU);
                Merge(A_WORLD);
                aw.GetListOfFiles(true);
                ArchiveBase.PurgeCache();
                A_BATTLE = ZZZ_MAIN;
                A_FIELD = ZZZ_MAIN;
                A_MAGIC = ZZZ_MAIN;
                A_MAIN = ZZZ_MAIN;
                A_MENU = ZZZ_MAIN;
                A_WORLD = ZZZ_MAIN;
            }
        }
    }
}