using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII
{
    public abstract class ArchiveBase
    {
        #region Fields

        protected const int MaxLocalCache = 10;
        protected static ConcurrentDictionary<string, BufferWithAge> LocalCache = new ConcurrentDictionary<string, BufferWithAge>();
        protected Memory.Archive _path;

        /// <summary>
        /// Generated File List
        /// </summary>
        protected string[] FileList;

        protected bool isDir = false;
        private const int MaxInCache = 50;
        private static ConcurrentDictionary<Memory.Archive, ArchiveBase> ArchiveCache = new ConcurrentDictionary<Memory.Archive, ArchiveBase>();

        #endregion Fields

        #region Properties

        public TimeSpan Created { get; protected set; }

        public TimeSpan Used { get; protected set; }

        protected static IOrderedEnumerable<KeyValuePair<string, BufferWithAge>> OrderByAge => LocalCache.OrderBy(x => x.Value.Touched).ThenBy(x => x.Key.Length).ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase);

        private static IEnumerable<KeyValuePair<Memory.Archive, ArchiveBase>> NonDirOrZZZ => ArchiveCache.Where(x => !x.Value.isDir && x.Value.GetType().Equals(typeof(ArchiveZZZ)));

        private static IEnumerable<KeyValuePair<Memory.Archive, ArchiveBase>> Oldest => NonDirOrZZZ.OrderBy(x => x.Value.Used).ThenBy(x => x.Value.Created);

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
            if (all)
                ArchiveCache.ForEach(x => ArchiveCache.TryRemove(x.Key, out ArchiveBase value));
            else
                NonDirOrZZZ.ForEach(x => ArchiveCache.TryRemove(x.Key, out ArchiveBase value));
        }

        public abstract ArchiveBase GetArchive(string fileName);

        public void GetArchive(Memory.Archive archive, out byte[] FI, out byte[] FS, out byte[] FL)
        {
            Memory.Log.WriteLine($"{nameof(ArchiveBase)}::{nameof(GetArchive)} - Reading: {archive.FI}, {archive.FS}, {archive.FL}");
            FI = GetBinaryFile(archive.FI);
            FS = GetBinaryFile(archive.FS);
            FL = GetBinaryFile(archive.FL);
        }

        public abstract ArchiveBase GetArchive(Memory.Archive archive);

        public abstract byte[] GetBinaryFile(string fileName, bool cache = false);

        public abstract string[] GetListOfFiles();

        public abstract Memory.Archive GetPath();

        public abstract StreamWithRangeValues GetStreamWithRangeValues(string filename);

        protected static bool LocalTryAdd(string Key, BufferWithAge Value)
        {
            if (LocalCache.TryAdd(Key, Value))
            {
                int left = 0;
                if ((left = OrderByAge.Count() - MaxLocalCache) > 0)
                {
                    OrderByAge.Take(left).ForEach(x => LocalCache.TryRemove(x.Key, out BufferWithAge tmp));
                }
                return true;
            }
            return false;
        }

        protected static bool TryAdd(Memory.Archive path, ArchiveBase value)
        {
            if (ArchiveCache.TryAdd(path, value))
            {
                value.Used = Memory.gameTime?.TotalGameTime ?? TimeSpan.Zero;
                value.Created = Memory.gameTime?.TotalGameTime ?? TimeSpan.Zero;
                int overage = 0;
                if ((overage = Oldest.Count() - MaxInCache) > 0)
                    Oldest.Take(overage).ForEach(x => ArchiveCache.TryRemove(x.Key, out ArchiveBase tmp));
                return true;
            }
            else return false;
        }

        protected static bool TryGetValue(Memory.Archive path, out ArchiveBase value)
        {
            if (ArchiveCache.TryGetValue(path, out value))
            {
                value.Used = Memory.gameTime?.TotalGameTime ?? TimeSpan.Zero;
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
                value.Poke();
                return true;
            }
            return false;
        }

        #endregion Methods
    }
}