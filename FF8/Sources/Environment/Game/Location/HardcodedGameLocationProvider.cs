using System;
using System.IO;

namespace FF8
{
    internal sealed class HardcodedGameLocationProvider
    {
        private readonly String[] _knownPaths;

        internal HardcodedGameLocationProvider(String[] knownPaths)
        {
            _knownPaths = knownPaths ?? throw new ArgumentNullException(nameof(knownPaths));
        }

        internal Boolean FindGameLocation(out GameLocation gameLocation)
        {
            using (var errors = new ExceptionList())
            {
                foreach (var path in _knownPaths)
                {
                    try
                    {
                        if (!Directory.Exists(path))
                            continue;

                        gameLocation = new GameLocation(path);
                        errors.Clear();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                    }
                }
            }

            gameLocation = null;
            return false;
        }
    }
}