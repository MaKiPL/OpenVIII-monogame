using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OpenVIII;
namespace DumpStrings
{
    internal class Program
    {
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
                XmlWriter.Create(new FileStream("strings.xml", FileMode.Create, FileAccess.Write, FileShare.ReadWrite),
                    xmlWriterSettings))
            {
                w.WriteStartDocument();
                w.WriteStartElement("Strings");//<Strings>
                var strings = (from s in Memory.Strings
                    from sb in s.Value
                    from r in sb.Value.Select((x,i)=> new {Key=i,Value=x})
                    where r.Value != null && r.Value.Length > 0
                    select new {sKey= s.Key,sbKey = sb.Key,id = r.Key, r.Value}).ToList().AsReadOnly();
                foreach (var sgroup in strings.GroupBy(x=>x.sKey))
                {
                    w.WriteStartElement("File");
                    w.WriteAttributeString("id",sgroup.Key.ToString());
                    foreach (var sbgroup in sgroup.GroupBy(x => x.sbKey))
                    {
                        w.WriteStartElement("Section");
                        w.WriteAttributeString("id", sbgroup.Key.ToString());
                        foreach (var s in sbgroup)
                        {
                            w.WriteStartElement("String");
                            w.WriteAttributeString("id", s.id.ToString());
                            w.WriteAttributeString("offset", s.Value.Offset.ToString("X"));
                            w.WriteString(s.Value);
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
    }
}
