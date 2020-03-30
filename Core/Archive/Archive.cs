using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// ReSharper disable InconsistentNaming

namespace OpenVIII
{
    public static partial class Memory
    {
        #region Classes

        /// <summary>
        /// Archive class handles the filename formatting and extensions for archive files.
        /// </summary>
        public class Archive : IReadOnlyList<string>
        {
            #region Fields

            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileArchive = ".fs";

            /// <summary>
            /// File Index Extension
            /// </summary>
            public const string B_FileIndex = ".fi";

            /// <summary>
            /// File Archive Extension
            /// </summary>
            public const string B_FileList = ".fl";

            public const string B_ZZZ = ".zzz";
            private readonly string[] _ext = { B_FileList, B_FileIndex, B_FileArchive, B_ZZZ };
            private string _filename;

            #endregion Fields

            #region Constructors

            public Archive(string path, params Archive[] parent) : this(path, false) => Parent = parent.ToList();

            public Archive(string path, bool keepExtension, params Archive[] parent) : this(path, keepExtension) => Parent = parent.ToList();

            public Archive(string path) : this(path, false)
            { }

            public Archive(string path, bool keepExtension)
            {
                Parent = null;
                SetFilename(path, keepExtension);
            }

            #endregion Constructors

            #region Properties

            public int Count => ((IReadOnlyList<string>)_ext).Count;

            /// <summary>
            /// File Index
            /// </summary>
            public string FI => $"{NoExtension}{B_FileIndex}";

            /// <summary>
            /// File List
            /// </summary>
            public string FL => $"{NoExtension}{B_FileList}";

            /// <summary>
            /// File Archive
            /// </summary>
            public string FS => $"{NoExtension}{B_FileArchive}";

            public bool IsDir { get; set; }
            public bool IsFile { get; set; }
            public bool IsFileArchive => _filename.EndsWith(B_FileArchive, StringComparison.OrdinalIgnoreCase);
            public bool IsFileIndex => _filename.EndsWith(B_FileIndex, StringComparison.OrdinalIgnoreCase);
            public bool IsFileList => _filename.EndsWith(B_FileList, StringComparison.OrdinalIgnoreCase);
            public bool IsZZZ => _filename.EndsWith(B_ZZZ, StringComparison.OrdinalIgnoreCase);
            public List<Archive> Parent { get; set; }

            /// <summary>
            /// ZZZ File
            /// </summary>
            public string ZZZ => $"{NoExtension}{B_ZZZ}";

            private string NoExtension => !string.IsNullOrWhiteSpace(_filename) ? Path.Combine(Path.GetDirectoryName(_filename) ?? throw new InvalidOperationException(), Path.GetFileNameWithoutExtension(_filename)) : "";

            #endregion Properties

            #region Indexers

            public string this[int index] => ((IReadOnlyList<string>)_ext)[index];

            #endregion Indexers

            #region Methods

            public static implicit operator Archive(string path) => new Archive(path);

            public static implicit operator string(Archive val) => val._filename;

            public IEnumerator<string> GetEnumerator() => ((IReadOnlyList<string>)_ext).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => ((IReadOnlyList<string>)_ext).GetEnumerator();

            /// <summary>
            /// SetFileName can be used to update path.
            /// </summary>
            /// <param name="path">Path to search parent for.</param>
            /// <param name="keepExtension">Extensions for FI FL FS are all different. Where ZZZ would be just one extension</param>
            public void SetFilename(string path, bool keepExtension = false)
            {
                if (Directory.Exists(path))
                {
                    IsDir = true;
                    _filename = path;
                }
                else if (File.Exists(path))
                {
                    IsFile = true;
                    _filename = path;
                }
                else if (!keepExtension)
                    _filename = Path.GetFileNameWithoutExtension(path);
                else
                    _filename = Path.GetFileName(path);
            }

            public override string ToString() => this;

            #endregion Methods
        }

        #endregion Classes
    }
}