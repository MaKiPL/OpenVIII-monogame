using System;
using System.Collections.Generic;
using System.IO;
// ReSharper disable StringLiteralTypo

namespace FF8.Fields
{
    public static partial class Field
    {
        public sealed class FSLookupService : ILookupService
        {
            private readonly String _rootPath;

            public FSLookupService(String rootPath)
            {
                if (!Directory.Exists(rootPath))
                    throw new DirectoryNotFoundException(rootPath);

                _rootPath = rootPath;
            }

            public IEnumerable<Info> EnumerateAll()
            {
                return Enumerate("*.mim");
            }

            public IEnumerable<Info> Enumerate(String mask)
            {
                foreach (String filePath in Directory.EnumerateFiles(_rootPath, mask, SearchOption.AllDirectories))
                {
                    String fieldName = Path.GetFileNameWithoutExtension(filePath);
                    if (SkipInvalidFields(fieldName))
                        continue;

                    IDataProvider dataProvider = new FsDataProvider(filePath);
                    yield return new Info(fieldName, dataProvider);
                }
            }

            private static Boolean SkipInvalidFields(String fieldName)
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
        }
    }
}