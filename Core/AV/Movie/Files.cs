using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Movie
    {
        public class Files : IReadOnlyList<string>
        {
            #region Fields

            private static readonly IReadOnlyList<string> Extensions = new[] { ".avi", ".mkv", ".mp4", ".bik" }.ToList().AsReadOnly();

            private readonly IReadOnlyList<string> _files;

            #endregion Fields

            #region Constructors

            private Files()
            {
                var a = (ArchiveZzz)ArchiveZzz.Load(Memory.Archives.ZZZ_OTHER);
                List<string> files;
                if (a != null)
                {
                    var listOfFiles = a.GetListOfFiles();
                    files = (from file in listOfFiles
                             from extension in Extensions
                             where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                             orderby Path.GetFileNameWithoutExtension(file)
                             select file).ToList();
                    ZZZ = true;
                }
                else
                {
                    //Gather all movie files.
                    var d = Directories.Instance;
                    files = (from directory in d
                             where Directory.Exists(directory)
                             from file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                             from extension in Extensions
                             where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                             orderby Path.GetFileNameWithoutExtension(file)
                             select file).ToList();
                }

                //Remove duplicate movies ignoring extension that have same name.
                (from s1 in files.Select((value, key) => new { Key = key, Value = value })
                 from s2 in files.Select((value, key) => new { Key = key, Value = value })
                 where s1?.Value != null
                 where s2?.Value != null
                 where s1.Key < s2.Key
                 where Path.GetFileNameWithoutExtension(s1.Value ?? throw new NullReferenceException($"{nameof(Files)}::{s1} value cannot be null")).Equals(Path.GetFileNameWithoutExtension(s2.Value), StringComparison.OrdinalIgnoreCase)
                 orderby s2.Key descending
                 select s2.Key).ForEach(key => files.RemoveAt(key));

                foreach (var s in files)
                    Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Files)} :: {s} ");
                _files = files.AsReadOnly();
            }

            #endregion Constructors

            #region Properties

            public static Files Instance { get; } = new Files();

            public int Count =>
                    _files.Count;

            public bool ZZZ { get; }

            #endregion Properties

            #region Indexers

            public string this[int index] => _files[index];

            #endregion Indexers

            #region Methods

            public bool Exists(int i) => Count > i && i >= 0 && File.Exists(_files[i]);

            public IEnumerator<string> GetEnumerator() => _files.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_files).GetEnumerator();

            #endregion Methods
        }
    }
}