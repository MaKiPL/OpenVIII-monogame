using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public abstract class ArchiveBase : IReadOnlyDictionary<string, byte[]>, IEnumerator<KeyValuePair<string, byte[]>>
    {
        #region Fields

        protected Memory.Archive Archive;

        /// <summary>
        /// Generated File List
        /// </summary>
        protected string[] FileList;

        private const int MaxInCache = 5;
        private const int MaxLocalCache = 5;
        private static readonly ConcurrentDictionary<string, ArchiveBase> Cache = new ConcurrentDictionary<string, ArchiveBase>();
        private static readonly object LocalAddLock = new object();
        private static readonly ConcurrentDictionary<string, BufferWithAge> LocalCache = new ConcurrentDictionary<string, BufferWithAge>();

        #endregion Fields

        #region Properties

        public ArchiveMap ArchiveMap { get; protected set; }

        public int Count => GetListOfFiles().Length;

        public DateTime Created { get; protected set; }

        public KeyValuePair<string, byte[]> Current => GetCurrent();

        object IEnumerator.Current => Enumerator;
        public bool IsDir { get; protected set; } = false;

        public bool IsOpen { get; protected set; } = false;

        public IEnumerable<string> Keys => GetListOfFiles();

        public DateTime Used { get; protected set; }

        public IEnumerable<byte[]> Values => GetListOfFiles().Select(x => GetBinaryFile(x));

        protected static IOrderedEnumerable<KeyValuePair<string, BufferWithAge>> OrderByAge => LocalCache.Where(x => x.Value != null).OrderBy(x => x.Value.Used).ThenBy(x => x.Key.Length).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

        protected IEnumerator Enumerator { get; set; }
        protected List<Memory.Archive> ParentPath { get; set; }

        private static IEnumerable<KeyValuePair<string, ArchiveBase>> NonDirOrZzz => Cache.Where(x => x.Value != null && !x.Value.IsDir && !(x.Value is ArchiveZzz));

        private static IEnumerable<KeyValuePair<string, ArchiveBase>> Oldest => NonDirOrZzz.OrderBy(x => x.Value.Used).ThenBy(x => x.Value.Created);

        #endregion Properties

        #region Indexers

        public byte[] this[string key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;
                return null;
            }
        }

        #endregion Indexers

        #region Methods

        public static ArchiveBase Load(Memory.Archive archive)
        {
            if (archive.IsDir)
            {
                return ArchiveWorker.Load(archive);
            }

            if (archive.IsFile)
            {
                if (archive.IsFileArchive || archive.IsFileIndex || archive.IsFileList)
                {
                    var a = new Memory.Archive(Path.GetFileNameWithoutExtension(archive), Path.GetDirectoryName(archive));
                    return ArchiveWorker.Load(a);
                }

                if (archive.IsZZZ)
                {
                    return ArchiveZzz.Load(archive);
                }

                return ArchiveWorker.Load(archive);
            }

            return null;
        }

        public static void PurgeCache(bool all = false)
        {
            LocalCache.ForEach(x =>
            {
                if (LocalCache.TryRemove(x.Key, out var _))
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                }
            });
            NonDirOrZzz.ForEach(x =>
            {
                if (Cache.TryRemove(x.Key, out var _))
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                }
            });

            if (all)
            {
                Cache.ForEach(x =>
                {
                    if (Cache.TryRemove(x.Key, out var _))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                    }
                });
            }
        }

        public bool ContainsKey(string key) => FindFile(ref key) > -1;

        public void Dispose()
        { }

        public abstract ArchiveBase GetArchive(string fileName);

        public void GetArchive(Memory.Archive archive, out StreamWithRangeValues fi, out ArchiveBase fs, out StreamWithRangeValues fl)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            fi = GetStreamWithRangeValues(archive.FI);
            fs = this;
            fl = GetStreamWithRangeValues(archive.FL);
        }

        public void GetArchive(Memory.Archive archive, out byte[] fi, out byte[] fs, out byte[] fl)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            fi = GetBinaryFile(archive.FI);
            fs = GetBinaryFile(archive.FS, cache: true);
            fl = GetBinaryFile(archive.FL);
        }

        public abstract ArchiveBase GetArchive(Memory.Archive archive);

        public abstract byte[] GetBinaryFile(string fileName, bool cache = false);

        public IEnumerator<KeyValuePair<string, byte[]>> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => this;

        /// <summary>
        /// Get current file list for loaded archive.
        /// </summary>
        public virtual string[] GetListOfFiles(bool force = false)
        {
            if (FileList != null && !force) return FileList;
            FileList = ProduceFileLists();
            Enumerator = FileList?.GetEnumerator();
            return FileList;
        }

        public virtual int GetMaxSize(Memory.Archive archive)
        {
            using (var s = GetStreamWithRangeValues(archive.FS))
                return checked((int)s.Size);
        }

        public abstract Memory.Archive GetPath();

        public abstract StreamWithRangeValues GetStreamWithRangeValues(string filename);

        public bool MoveNext() => (GetListOfFiles()?.Length ?? 0) > 0 && Enumerator.MoveNext();

        public void Reset()
        {
            var list = GetListOfFiles();
            if (list != null && list.Length > 0)
                Enumerator.Reset();
        }

        public override string ToString() => $"{Archive} :: {Used}";

        public bool TryGetValue(string key, out byte[] value) => (value = GetBinaryFile(key)) != null;

        protected static bool CacheTryAdd(string key, ArchiveBase value)
        {
            if (!Cache.TryAdd(key, value)) return false;
            if (value != null) value.Used = value.Created = DateTime.Now;

            if ((Oldest.Count() - MaxInCache) > 0)
                Oldest.Where(x => x.Key != key).Reverse().Skip(MaxInCache)
                    .ForEach(x => Cache.TryRemove(x.Key, out var _));
            return true;
        }

        protected static bool CacheTryGetValue(string path, out ArchiveBase value)
        {
            if (!Cache.TryGetValue(path, out value)) return false;
            if (value != null)
                value.Used = DateTime.Now;
            return true;
        }

        protected static bool LocalTryAdd(string key, BufferWithAge value)
        {
            lock (LocalAddLock)
            {
                Debug.Assert(!key.EndsWith("field.fs", StringComparison.OrdinalIgnoreCase));
                if (!LocalCache.TryAdd(key, value)) return false;
                if ((OrderByAge.Count() - MaxLocalCache) > 0)
                {
                    OrderByAge.Where(x => x.Key != key).Reverse().Skip(MaxLocalCache).ForEach(x =>
                    {
                        if (LocalCache.TryRemove(x.Key, out var _))
                        {
                            Memory.Log.WriteLine(
                                $"{nameof(ArchiveBase)}::{nameof(LocalTryAdd)}::Evicting: \"{x.Key}\"");
                        }
                    });
                }
                return true;
            }
        }

        protected virtual int FindFile(ref string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) return -1;
            return ArchiveMap != null && ArchiveMap.Count > 1
                ? ArchiveMap.FindString(ref filename, out var _) == default ? -1 : 0
                : -1;
        }

        protected List<Memory.Archive> FindParentPath(Memory.Archive path)
        {
            if (path.Parent == null) return null;
            foreach (var pathParent in path.Parent)
            {
                if (pathParent.IsDir)
                    return new List<Memory.Archive> { pathParent };
                if (pathParent.IsFile)
                    return new List<Memory.Archive> { pathParent };
                if (pathParent.Parent == null || pathParent.Parent.Count <= 0) continue;
                var returnList = FindParentPath(pathParent);
                if (returnList == null || returnList.Count <= 0) continue;
                returnList.Add(pathParent);
                return returnList;
            }
            return null;
        }

        protected bool LocalTryGetValue(string filename, out BufferWithAge value)
        {
            if (!LocalCache.TryGetValue(filename, out value)) return false;
            value?.Poke();
            return true;
        }

        protected virtual string[] ProduceFileLists() =>
            ArchiveMap != null && ArchiveMap.Count > 0
                ? ArchiveMap.OrderedByName.Select(x => x.Key).ToArray()
                : null;

        private KeyValuePair<string, byte[]> GetCurrent()
        {
            var s = (string)(Enumerator.Current);
            return new KeyValuePair<string, byte[]>(s, GetBinaryFile(s));
        }

        #endregion Methods
    }
}