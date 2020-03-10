using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Movie
    {
        public class Files : IEnumerable<string>
        {
            private Files()
            {

                if (_files != null) return;
                if(_extensions == null)
                    _extensions = new [] { ".avi", ".mkv", ".mp4", ".bik" };
                ArchiveZzz a = (ArchiveZzz)ArchiveZzz.Load(Memory.Archives.ZZZ_OTHER);
                if (a != null)
                {
                    string[] listOfFiles = a.GetListOfFiles();
                    _files = (from file in listOfFiles
                              from extension in _extensions
                              where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                              orderby Path.GetFileNameWithoutExtension(file)
                              select file).ToList();
                    ZZZ = true;
                }
                else
                {
                    //Gather all movie files.
                    Directories d = Directories.Instance;
                    _files = (from directory in d
                              where Directory.Exists(directory)
                              from file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                              from extension in _extensions
                              where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                              orderby Path.GetFileNameWithoutExtension(file)
                              select file).ToList();
                }

                //Remove duplicate movies ignoring extension that have same name.
                (from s1 in _files.Select((value, key) => new { Key = key, Value = value })
                 from s2 in _files.Select((value, key) => new { Key = key, Value = value })
                 where s1?.Value != null
                 where s2?.Value != null
                 where s1.Key < s2.Key
                 where Path.GetFileNameWithoutExtension(s1.Value ?? throw new NullReferenceException($"{nameof(Files)}::{s1} value cannot be null")).Equals(Path.GetFileNameWithoutExtension(s2.Value), StringComparison.OrdinalIgnoreCase)
                 orderby s2.Key descending
                 select s2.Key).ForEach(key => _files.RemoveAt(key));

                foreach (string s in _files)
                    Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Files)} :: {s} ");
            }

            public static Files Instance { get; } = new Files();

            #region Fields

            private static string[] _extensions;
            private static List<string> _files;

            public static bool ZZZ { get; set; }

            #endregion Fields

            #region Properties

            public int Count =>
                    _files.Count;

            #endregion Properties

            #region Indexers

            public string this[int i] => At(i);

            #endregion Indexers

            #region Methods

            public string At(int i) => _files[i];

            public bool Exists(int i) => Count > i && i >= 0 && File.Exists(_files[i]);


            public IEnumerator GetEnumerator() => _files.GetEnumerator();

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => _files.GetEnumerator();

            #endregion Methods
        }
    }
}