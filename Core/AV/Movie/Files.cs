using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    namespace Movie
    {
        public struct Files : IEnumerable<string>
        {
            #region Fields

            private static readonly string[] Extensions = new string[] { ".avi", ".mkv", ".mp4", ".bik" };
            private static Directories Directories;
            private static List<string> s_files;

            public static bool ZZZ { get; private set; } = false;

            #endregion Fields

            #region Properties

            public static int Count => _Files.Count;
            private static List<string> _Files { get { Init(); return s_files; } set => s_files = value; }

            #endregion Properties

            #region Indexers

            public string this[int i] => At(i);

            #endregion Indexers

            #region Methods

            public static string At(int i) => _Files[i];

            public static bool Exists(int i) => Count > i && i >= 0 && File.Exists(_Files[i]);

            public static void Init()
            {
                if (s_files == null /*|| s_files.Count == 0*/)
                {
                    ArchiveZZZ a = (ArchiveZZZ)ArchiveZZZ.Load(Memory.Archives.ZZZ_OTHER);
                    if (a != null)
                    {

                        s_files = (from file in a.GetListOfFiles()
                                   from extension in Extensions
                                   where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                                   orderby Path.GetFileNameWithoutExtension(file) ascending
                                   select file).ToList();
                        ZZZ = true;
                    }
                    else
                        //Gather all movie files.
                        s_files = (from directory in Directories
                                   where Directory.Exists(directory)
                                   from file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)
                                   from extension in Extensions
                                   where file.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                                   orderby Path.GetFileNameWithoutExtension(file) ascending
                                   select file).ToList();
                    //Remove duplicate movies ignoring extension that have same name.
                    (from s1 in _Files.Select((Value, Key) => new { Key, Value })
                     from s2 in _Files.Select((Value, Key) => new { Key, Value })
                     where s1.Key < s2.Key
                     where Path.GetFileNameWithoutExtension(s1.Value).Equals(Path.GetFileNameWithoutExtension(s2.Value), StringComparison.OrdinalIgnoreCase)
                     orderby s2.Key descending
                     select s2.Key).ForEach(Key => s_files.RemoveAt(Key));

                    foreach (string s in s_files)
                        Memory.Log.WriteLine($"{nameof(Movie)} :: {nameof(Files)} :: {s} ");
                }
            }

            public IEnumerator GetEnumerator() => _Files.GetEnumerator();

            IEnumerator<string> IEnumerable<string>.GetEnumerator() => _Files.GetEnumerator();

            #endregion Methods
        }
    }
}