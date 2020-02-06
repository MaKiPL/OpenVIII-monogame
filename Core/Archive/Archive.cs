using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#pragma warning disable CS0649

namespace OpenVIII
{
    public static partial class Memory
    {
        /// <summary>
        /// Archive class handles the filename formatting and extensions for archive files.
        /// </summary>
        public class Archive : IReadOnlyList<string>
        {
            public List<Archive> Parent { get; set; }
            public bool IsDir { get; }
            public bool IsFile { get; }

            //public string _Root { get; set; }
            public string _Filename { get; private set; }

            public Archive(string path, params Archive[] parent) : this(path)
            {
                Parent = parent.ToList();
            }

            public Archive(string path)
            {
                Parent = null;
                if (Directory.Exists(path))
                {
                    IsDir = true;
                    _Filename = path;
                }
                else if (File.Exists(path))
                {
                    IsFile = true;
                    _Filename = path;
                }
                else
                    _Filename = Path.GetFileNameWithoutExtension(path);
            }

            public static implicit operator string(Archive val) => val._Filename;

            public static implicit operator Archive(string path) => new Archive(path);
            private readonly string[] ext = new string[] { B_FileList, B_FileIndex, B_FileArchive, B_ZZZ };
            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileList = ".fl";

            /// <summary>
            /// File Index Extension
            /// </summary>
            public const string B_FileIndex = ".fi";

            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileArchive = ".fs";

            public const string B_ZZZ = ".zzz";

            public bool IsZZZ => _Filename.EndsWith(B_ZZZ, StringComparison.OrdinalIgnoreCase);
            public bool IsFileList => _Filename.EndsWith(B_FileList, StringComparison.OrdinalIgnoreCase);
            public bool IsFileIndex => _Filename.EndsWith(B_FileIndex, StringComparison.OrdinalIgnoreCase);
            public bool IsFileArchive => _Filename.EndsWith(B_FileArchive, StringComparison.OrdinalIgnoreCase);
            public string RemoveExtension
            {
                get
                {
                    int startIndex = _Filename.LastIndexOf('.');
                    if(startIndex != -1)
                    return _Filename.Remove(startIndex, _Filename.Length - startIndex);
                    return _Filename;
                }
            }
            /// <summary>
            /// File Index
            /// </summary>
            public string FI => $"{_Filename}{B_FileIndex}";

            /// <summary>
            /// File List
            /// </summary>
            public string FL => $"{_Filename}{B_FileList}";

            /// <summary>
            /// File Archive
            /// </summary>
            public string FS => $"{_Filename}{B_FileArchive}";

            /// <summary>
            /// ZZZ File
            /// </summary>
            public string ZZZ => $"{_Filename}{B_ZZZ}";

            public int Count => ((IReadOnlyList<string>)ext).Count;

            public string this[int index] => ((IReadOnlyList<string>)ext)[index];

            //public bool FileExistsInFolder { get; private set; }

            ///// <summary>
            ///// Test if input file path exists
            ///// </summary>
            ///// <param name="input">file path</param>
            ///// <returns></returns>
            //private string Test(string input)
            //{
            //    //using this for archives in archives would always throw exceptions
            //    if (!File.Exists(input)) FileExistsInFolder = false; //throw new FileNotFoundException($"There is no {input} file!\nExiting...");
            //    else FileExistsInFolder = true;
            //    return input;
            //}

            public override string ToString() => this;
            public IEnumerator<string> GetEnumerator() => ((IReadOnlyList<string>)ext).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<string>)ext).GetEnumerator();
        }
    }
}