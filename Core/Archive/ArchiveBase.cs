using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public abstract class ArchiveBase
    {
        #region Fields

        protected const int MaxLocalCache = 5;
        protected static ConcurrentDictionary<string, BufferWithAge> LocalCache = new ConcurrentDictionary<string, BufferWithAge>();
        protected Memory.Archive _path;

        /// <summary>
        /// Generated File List
        /// </summary>
        protected string[] FileList;

        private const int MaxInCache = 5;
        private static ConcurrentDictionary<string, ArchiveBase> ArchiveCache = new ConcurrentDictionary<string, ArchiveBase>();
        private static object localaddlock = new object();

        #endregion Fields

        #region Properties

        public ArchiveMap ArchiveMap { get; protected set; }
        public DateTime Created { get; protected set; }
        public bool isDir { get; protected set; } = false;
        public bool IsOpen { get; protected set; } = false;
        public DateTime Used { get; protected set; }

        protected static IOrderedEnumerable<KeyValuePair<string, BufferWithAge>> OrderByAge => LocalCache.Where(x => x.Value != null).OrderBy(x => x.Value.Used).ThenBy(x => x.Key.Length).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

        private static IEnumerable<KeyValuePair<string, ArchiveBase>> NonDirOrZZZ => ArchiveCache.Where(x => x.Value != null && !x.Value.isDir && !(x.Value is ArchiveZZZ));

        private static IEnumerable<KeyValuePair<string, ArchiveBase>> Oldest => NonDirOrZZZ.OrderBy(x => x.Value.Used).ThenBy(x => x.Value.Created);

        #endregion Properties

        #region Methods

        public static ArchiveBase Load(Memory.Archive archive)
        {
            if (archive.IsDir)
            {
                return ArchiveWorker.Load(archive);
            }
            else if (archive.IsFile)
            {
                if (archive.IsFileArchive || archive.IsFileIndex || archive.IsFileList)
                {
                    Memory.Archive a = new Memory.Archive(Path.GetFileNameWithoutExtension(archive), Path.GetDirectoryName(archive));
                    return ArchiveWorker.Load(a);
                }
                else if (archive.IsZZZ)
                {
                    return ArchiveZZZ.Load(archive);
                }
                else
                    return ArchiveWorker.Load(archive);
            }
            else
                return null;
        }

        public static void PurgeCache(bool all = false)
        {
            LocalCache.ForEach(x =>
            {
                if (LocalCache.TryRemove(x.Key, out BufferWithAge tmp))
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                }
            });
            NonDirOrZZZ.ForEach(x =>
            {
                if (ArchiveCache.TryRemove(x.Key, out ArchiveBase tmp))
                {
                    Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                }
            });

            if (all)
            {
                ArchiveCache.ForEach(x =>
                {
                    if (ArchiveCache.TryRemove(x.Key, out ArchiveBase tmp))
                    {
                        Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(PurgeCache)}::Evicting: \"{x.Key}\"");
                    }
                });
            }
        }

        public abstract ArchiveBase GetArchive(string fileName);

        public void GetArchive(Memory.Archive archive, out StreamWithRangeValues FI, out ArchiveBase FS, out StreamWithRangeValues FL)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            FI = GetStreamWithRangeValues(archive.FI);
            FS = this;
            FL = GetStreamWithRangeValues(archive.FL);
        }

        public void GetArchive(Memory.Archive archive, out byte[] FI, out byte[] FS, out byte[] FL)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            FI = GetBinaryFile(archive.FI);
            FS = GetBinaryFile(archive.FS, cache: true);
            FL = GetBinaryFile(archive.FL);
        }

        public abstract ArchiveBase GetArchive(Memory.Archive archive);

        public abstract byte[] GetBinaryFile(string fileName, bool cache = false);

        public abstract string[] GetListOfFiles();

        public abstract Memory.Archive GetPath();

        public abstract StreamWithRangeValues GetStreamWithRangeValues(string filename, FI fi = null, int size = 0);

        public override string ToString() => $"{_path} :: {Used}";

        protected static bool LocalTryAdd(string Key, BufferWithAge Value)
        {
            lock (localaddlock)
            {
                Debug.Assert(!Key.EndsWith("field.fs", StringComparison.OrdinalIgnoreCase));
                if (LocalCache.TryAdd(Key, Value))
                {
                    int left = 0;
                    if ((left = OrderByAge.Count() - MaxLocalCache) > 0)
                    {
                        OrderByAge.Where(x => x.Key != Key).Reverse().Skip(MaxLocalCache).ForEach(x =>
                          {
                              if (LocalCache.TryRemove(x.Key, out BufferWithAge tmp))
                              {
                                  Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(LocalTryAdd)}::Evicting: \"{x.Key}\"");
                              }
                          });
                    }
                    return true;
                }
                return false;
            }
        }

        protected static bool TryAdd(string Key, ArchiveBase value)
        {
            if (ArchiveCache.TryAdd(Key, value))
            {
                if (value != null)
                {
                    value.Used = value.Created = DateTime.Now;
                }
                int overage = 0;
                if ((overage = Oldest.Count() - MaxInCache) > 0)
                    Oldest.Where(x => x.Key != Key).Reverse().Skip(MaxInCache).ForEach(x => ArchiveCache.TryRemove(x.Key, out ArchiveBase tmp));
                return true;
            }
            else return false;
        }

        protected static bool TryGetValue(string path, out ArchiveBase value)
        {
            if (ArchiveCache.TryGetValue(path, out value))
            {
                if (value != null)
                    value.Used = DateTime.Now;
                return true;
            }
            else return false;
        }

        protected List<Memory.Archive> FindParentPath(Memory.Archive path)
        {
            if (path.Parent != null)
                foreach (Memory.Archive pathParent in path.Parent)
                {
                    if (pathParent.IsDir)
                        return new List<Memory.Archive> { pathParent };
                    else if (pathParent.IsFile)
                        return new List<Memory.Archive> { pathParent };
                    else if (pathParent.Parent != null && pathParent.Parent.Count > 0)
                    {
                        List<Memory.Archive> returnList = FindParentPath(pathParent);
                        if (returnList != null && returnList.Count > 0)
                        {
                            returnList.Add(pathParent);
                            return returnList;
                        }
                    }
                }
            return null;
        }

        protected bool LocalTryGetValue(string filename, out BufferWithAge value)
        {
            if (LocalCache.TryGetValue(filename, out value))
            {
                if (value != null)
                    value.Poke();
                return true;
            }
            return false;
        }

        #endregion Methods
    }
}