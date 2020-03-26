using OpenVIII;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DumpStrings
{
    internal class Program
    {
        #region Properties

        private static XmlWriterSettings XmlWriterSettings { get; } = new XmlWriterSettings()
        {
            Indent = true,
            IndentChars = "\t",
            NewLineOnAttributes = true,
            Encoding = Encoding.Unicode
        };

        private static IEnumerable<(Strings.FileID fileKey, int sectionKey, int id, FF8StringReference Value)> CollectionOfStrings
        {
            get
            {
                (Strings.FileID fileKey, int sectionKey, int id, FF8StringReference Value) Get(Strings.FileID fileID,
                    int sectionID, int inputID, FF8StringReference value)
                    => (fileID, sectionID, inputID, value); // this is for having a tuple instead of anonymous type

                var strings = (
                    from file in Memory.Strings
                    from section in file.Value
                    from ff8StringReference in section.Value.Select((x, i) => new { Key = i, Value = x })
                    where ff8StringReference.Value != null && ff8StringReference.Value.Length > 0
                    select
                        Get(file.Key, section.Key, ff8StringReference.Key, ff8StringReference.Value)
                ).ToList().AsReadOnly();
                return strings;
            }
        }

        #endregion Properties

        #region Methods

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        private static void DumpReverseStrings()
        {
            using (var w =
                XmlWriter.Create(
                    new FileStream($"reverse_strings_{Extended.GetLanguageShort().ToLower()}.xml", FileMode.Create, FileAccess.Write,
                        FileShare.ReadWrite),
                    XmlWriterSettings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("ReverseStrings"); //<ReverseStrings>
                w.WriteAttributeString("lang", Extended.GetLanguageShort().ToUpper());

                var strings = CollectionOfStrings;

                w.WriteAttributeString("count", strings.Count().ToString("D"));
                foreach ((var fileID, var sectionID, var id, var value) in strings)
                {
                    w.WriteStartElement("String");
                    w.WriteAttributeString(nameof(fileID).ToLower(), fileID.ToString("D"));
                    w.WriteAttributeString(nameof(sectionID).ToLower(), sectionID.ToString("D"));
                    w.WriteAttributeString(nameof(id)+$"_{Extended.GetLanguageShort().ToLower()}", id.ToString("D"));
                    w.WriteAttributeString(nameof(value.Offset).ToLower() + $"_{Extended.GetLanguageShort().ToLower()}", value.Offset.ToString("X"));
                    w.WriteAttributeString(nameof(value.Length).ToLower() + $"_{Extended.GetLanguageShort().ToLower()}", value.Length.ToString("D"));
                    w.WriteAttributeString(nameof(value) + $"_{Extended.GetLanguageShort().ToLower()}", value);
                    w.WriteEndElement();
                }
                w.WriteEndElement(); //</ReverseStrings>
                w.WriteEndDocument();
            }
        }

        private static void DumpStrings()
        {
            using (var w =
                XmlWriter.Create(
                    new FileStream($"strings_{Extended.GetLanguageShort().ToLower()}.xml", FileMode.Create, FileAccess.Write,
                        FileShare.ReadWrite),
                    XmlWriterSettings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("Strings"); //<Strings>
                w.WriteAttributeString("lang", Extended.GetLanguageShort().ToUpper());

                var strings = CollectionOfStrings;
                var fileGroups = strings.GroupBy(x => x.fileKey).ToList().AsReadOnly();
                //var fileGroupsCounts = fileGroups.Select(group => new {group.Key, Count = group.Count()});
                var fileGroupsCount = fileGroups.Count();
                w.WriteAttributeString("count", fileGroupsCount.ToString("D"));
                foreach (var fileGroup in fileGroups)
                {
                    w.WriteStartElement("File");
                    w.WriteAttributeString("id", fileGroup.Key.ToString());
                    var sectionGroups = fileGroup.GroupBy(x => x.sectionKey).ToList().AsReadOnly();
                    var sectionGroupsCount = sectionGroups.Count();
                    w.WriteAttributeString("count", sectionGroupsCount.ToString("D"));
                    foreach (var sectionGroup in sectionGroups)
                    {
                        w.WriteStartElement("Section");
                        w.WriteAttributeString("id", sectionGroup.Key.ToString());
                        w.WriteAttributeString("count", sectionGroup.Count().ToString("D"));
                        foreach (var ff8StringReference in sectionGroup)
                        {
                            w.WriteStartElement("String");
                            w.WriteAttributeString("id", ff8StringReference.id.ToString());
                            w.WriteAttributeString("offset", ff8StringReference.Value.Offset.ToString("X"));
                            w.WriteAttributeString("size", ff8StringReference.Value.Length.ToString("D"));
                            w.WriteString(ff8StringReference.Value);
                            w.WriteEndElement();
                        }

                        w.WriteEndElement();
                    }

                    w.WriteEndElement();
                }

                w.WriteEndElement(); //</Strings>
                w.WriteEndDocument();
            }
        }

        private static void Main(string[] args)
        {
            Memory.Init(null, null, null, args);
            DumpStrings();
            DumpReverseStrings();
        }

        #endregion Methods
    }
}