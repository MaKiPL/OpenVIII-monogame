using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace OpenVIII
{
    namespace Movie
    {
        public class Directories : IEnumerable<string>
        {
            private Directories()
            {
                if (_directories == null /*|| _directories.Count == 0*/)
                {
                    _directories = new List<string> {
                        Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata, "movies")), //this folder has most movies
                        Extended.GetUnixFullPath(Path.Combine(Memory.FF8DIRdata_lang, "movies")) //this folder has rest of movies
                    };
                    foreach (string s in _directories)
                        Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Directories)} :: {s} ");
                }
            }

            public static Directories Instance { get; } = new Directories();

            #region Fields

            private static List<string> _directories;

            #endregion Fields

            #region Properties

            public static int Count => _directories.Count;

            #endregion Properties

            #region Indexers

            public string this[int i] => _directories[i];

            #endregion Indexers

            #region Methods
            
            public IEnumerator<string> GetEnumerator() => _directories.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _directories.GetEnumerator();

            #endregion Methods
        }
    }
}