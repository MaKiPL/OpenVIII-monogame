using System.Collections.Generic;
using System.IO;

// ReSharper disable StringLiteralTypo

namespace OpenVIII.Fields
{
    public static partial class Field
    {
        #region Classes

        public sealed class FSLookupService : ILookupService
        {
            #region Fields

            private readonly string _rootPath;

            #endregion Fields

            #region Constructors

            public FSLookupService(string rootPath)
            {
                if (!Directory.Exists(rootPath))
                    throw new DirectoryNotFoundException(rootPath);

                _rootPath = rootPath;
            }

            #endregion Constructors

            #region Methods

            public IEnumerable<Info> Enumerate(string mask)
            {
                foreach (var filePath in Directory.EnumerateFiles(_rootPath, mask, SearchOption.AllDirectories))
                {
                    var fieldName = Path.GetFileNameWithoutExtension(filePath);
                    if (SkipInvalidFields(fieldName))
                        continue;

                    IDataProvider dataProvider = new FsDataProvider(filePath);
                    yield return new Info(fieldName, dataProvider);
                }
            }

            public IEnumerable<Info> EnumerateAll() => Enumerate("*.mim");

            private static bool SkipInvalidFields(string fieldName)
            {
                switch (fieldName)
                {
                    case "test":
                    case "test1":
                    case "test3":
                    case "test10":
                    case "test11":
                    case "test12":
                    case "bccent12":
                    case "bccent15":
                    case "bgroom_2":
                    case "bg2f_1a":
                    case "doani2_1":
                    case "doani2_2":
                    case "glclock2":
                    case "glsta3":
                    case "glsta4":
                    case "glstage2":
                    case "glwitch2":
                    case "glyagu2":
                    case "logo":
                        // Corrupted JSM: Test data, invalid stack
                        return true;

                    default:
                        return false;
                }
            }

            #endregion Methods
        }

        #endregion Classes
    }
}