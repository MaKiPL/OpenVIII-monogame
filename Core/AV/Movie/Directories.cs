using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Movie
    {
        public struct Directories : IEnumerable<string>
        {
            #region Fields

            private static List<string> s_directories;

            #endregion Fields

            #region Properties

            public int Count => _Directories.Count;
            private static List<string> _Directories { get { Init(); return s_directories; } set => s_directories = value; }

            #endregion Properties

            #region Indexers

            public string this[int i] => _Directories[i];

            #endregion Indexers

            #region Methods

            public static void Init()
            {
                if (s_directories == null /*|| s_directories.Count == 0*/)
                {
                    s_directories = new List<string> {
                        Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "movies")), //this folder has most movies
                        Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata_lang, "movies")) //this folder has rest of movies
                    };
                    foreach (var s in s_directories)
                        Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Directories)} :: {s} ");
                }
            }

            public IEnumerator<string> GetEnumerator() => _Directories.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _Directories.GetEnumerator();

            #endregion Methods
        }
    }
}