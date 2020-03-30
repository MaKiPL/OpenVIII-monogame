using System;
using System.IO;

namespace OpenVIII
{
    public sealed class HardcodedGameLocationProvider
    {
        private readonly string[] _knownPaths;

        public HardcodedGameLocationProvider(string[] knownPaths)
        {
            _knownPaths = knownPaths ?? throw new ArgumentNullException(nameof(knownPaths));
        }

        public bool FindGameLocation(out GameLocation gameLocation)
        {
            //using (var errors = new ExceptionList())
            //{
                foreach (var path in _knownPaths)
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            continue;

                        gameLocation = new GameLocation(path);
                        //errors.Clear();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        ex.Rethrow();
                        //errors.Add(ex);
                    }
                }
            //}

            gameLocation = null;
            return false;
        }
    }
}