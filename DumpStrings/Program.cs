using OpenVIII;
using System.IO;
using System.Linq;
using System.Xml;

namespace DumpStrings
{
    internal class Program
    {
        #region Methods

        private static void Main(string[] args)
        {
            Memory.Init(null, null, null, args);
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings()
            {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = true
            };
            using (XmlWriter w =
                XmlWriter.Create(new FileStream($"strings_{Extended.GetLanguageShort().ToLower()}.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite),
                    xmlWriterSettings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("Strings");//<Strings>
                w.WriteAttributeString("lang", Extended.GetLanguageShort().ToUpper());
                var strings = (
                    from file in Memory.Strings
                    from section in file.Value
                    from ff8StringReference in section.Value.Select((x, i) => new { Key = i, Value = x })
                    where ff8StringReference.Value != null && ff8StringReference.Value.Length > 0
                    select new { fileKey = file.Key, sectionKey = section.Key, id = ff8StringReference.Key, ff8StringReference.Value }).ToList().AsReadOnly();
                var fileGroups = strings.GroupBy(x => x.fileKey).ToList().AsReadOnly();
                //var fileGroupsCounts = fileGroups.Select(group => new {group.Key, Count = group.Count()});
                int fileGroupsCount = fileGroups.Count();
                w.WriteAttributeString("count", fileGroupsCount.ToString("D"));
                foreach (var fileGroup in fileGroups)
                {
                    w.WriteStartElement("File");
                    w.WriteAttributeString("id", fileGroup.Key.ToString());
                    var sectionGroups = fileGroup.GroupBy(x => x.sectionKey).ToList().AsReadOnly();
                    int sectionGroupsCount = sectionGroups.Count();
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
                            w.WriteAttributeString("size", ff8StringReference.Value.Length.ToString("X"));
                            w.WriteString(ff8StringReference.Value);
                            w.WriteEndElement();
                        }
                        w.WriteEndElement();
                    }
                    w.WriteEndElement();
                }
                w.WriteEndElement();//</Strings>
                w.WriteEndDocument();
            }
        }

        #endregion Methods
    }
}