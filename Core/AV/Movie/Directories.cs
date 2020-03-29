using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Movie
    {
        public sealed class Directories : IReadOnlyList<string>
        {
            #region Fields

            private readonly IReadOnlyList<string> _directories;

            #endregion Fields

            #region Constructors

            private Directories()
            {
                _directories = new []{
                    Extended.GetUnixFullPath(Path.Combine(Memory.FF8DirData, "movies")), //this folder has most movies
                    Extended.GetUnixFullPath(Path.Combine(Memory.FF8DirDataLang, "movies")) //this folder has rest of movies
                }.Distinct().ToList().AsReadOnly();
                foreach (var s in _directories)
                    Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Directories)} :: {s} ");
            }

            #endregion Constructors

            #region Properties

            public static Directories Instance { get; } = new Directories();
            public int Count => _directories.Count;

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